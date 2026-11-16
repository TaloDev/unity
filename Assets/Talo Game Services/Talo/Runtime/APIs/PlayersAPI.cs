using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class MergeOptions
    {
        public string postMergeIdentityService = "";
        public string player2ClaimKey = "";
    }

    public class PlayersAPI : DebouncedAPI<PlayersAPI.DebouncedOperation, RejectedProp[], PlayersAPI.PlayerUpdateResult>
    {
        public enum DebouncedOperation
        {
            Update
        }

        public event Action<PlayerAlias> OnIdentified;
        public event Action OnIdentificationStarted;
        public event Action<IdentifyException> OnIdentificationFailed;
        public event Action OnIdentityCleared;
        public event Action<bool> OnPlayerUpdated;

        public PlayersAPI() : base("v1/players")
        {
            Talo.OnConnectionRestored += OnConnectionRestored;
            OnOperationSettled += (success, _) => OnPlayerUpdated?.Invoke(success);
        }

        private async void OnConnectionRestored()
        {
            await Talo.Socket.ResetConnection();

            if (Talo.HasIdentity())
            {
                var socketToken = await CreateSocketToken();
                if (!string.IsNullOrEmpty(socketToken))
                {
                    Talo.Socket.SetSocketToken(socketToken);
                }
            }
        }

        public void InvokeIdentifiedEvent()
        {
            OnIdentified?.Invoke(Talo.CurrentAlias);
        }

        private async Task<PlayerAlias> HandleIdentifySuccess(PlayerAlias alias, string socketToken = "")
        {
            if (!Talo.IsOffline() && Talo.Socket.IsIdentified())
            {
                await Talo.Socket.ResetConnection();
            }

            Talo.CurrentAlias = alias;
            if (!string.IsNullOrEmpty(socketToken))
            {
                Talo.Socket.SetSocketToken(socketToken);
            }

            InvokeIdentifiedEvent();

            return alias;
        }

        public async Task<PlayerAlias> Identify(string service, string identifier, string claimKey = "")
        {
            OnIdentificationStarted?.Invoke();

            if (Talo.IsOffline())
            {
                return await IdentifyOffline(service, identifier);
            }

            // use the current player's claim key if one isn't supplied
            if (string.IsNullOrEmpty(claimKey))
            {
                claimKey = Talo.ClaimKeys.GetIdentifyKey(service, identifier);
            }

            var uri = new Uri($"{baseUrl}/identify");
            var content = JsonUtility.ToJson(new PlayersIdentifyRequest(service, identifier));

            PlayersIdentifyResponse res;
            try
            {
                // the player is not resolved yet, so send no identity headers
                var json = await Call(uri, "POST", content, claimKey: claimKey, sendIdentity: false);
                res = JsonUtility.FromJson<PlayersIdentifyResponse>(json);
            }
            catch (Exception ex)
            {
                var failure = IdentifyException.FromException(ex);
                if (failure.ErrorCode != IdentifyErrorCode.INVALID_CLAIM_KEY)
                {
                    await Talo.PlayerAuth.SessionManager.ClearSession();
                }

                OnIdentificationFailed?.Invoke(failure);
                throw;
            }

            Talo.ClaimKeys.SaveKey(res.alias.player.id, claimKey, res.alias.service, res.alias.identifier);
            res.alias.WriteOfflineAlias();
            return await HandleIdentifySuccess(res.alias, res.socketToken);
        }

        public async Task<PlayerAlias> IdentifySteam(string ticket, string identityClient = "", string claimKey = "")
        {
            if (string.IsNullOrEmpty(identityClient))
            {
                return await Identify("steam", ticket, claimKey);
            }
            else
            {
                return await Identify("steam", $"{identityClient}:{ticket}", claimKey);
            }
        }

        public async Task<PlayerAlias> IdentifyGooglePlayGames(string authCode, string claimKey = "")
        {
            return await Identify("google_play_games", authCode, claimKey);
        }

        public async Task<PlayerAlias> IdentifyGameCenter(
            string publicKeyUrl,
            byte[] signature,
            byte[] salt,
            ulong timestamp,
            string playerId,
            string claimKey = ""
        )
        {
            var payload = new PlayersGameCenterIdentifier
            {
                publicKeyUrl = publicKeyUrl,
                signature = Convert.ToBase64String(signature),
                salt = Convert.ToBase64String(salt),
                timestamp = timestamp,
                playerId = playerId,
                bundleId = Application.identifier
            };

            var identifier = JsonUtility.ToJson(payload);

            return await Identify("game_center", identifier, claimKey);
        }

        protected override async Task<RejectedProp[]> ExecuteDebouncedOperation(DebouncedOperation operation)
        {
            return operation switch
            {
                DebouncedOperation.Update => await RunUpdate(),
                _ => null,
            };
        }

        protected override PlayerUpdateResult BuildResult(bool success, RejectedProp[] updateData)
        {
            if (!success)
            {
                return new PlayerUpdateResult(false);
            }
            return new PlayerUpdateResult(true, updateData);
        }

        public Task<PlayerUpdateResult> DebounceUpdate()
        {
            return Debounce(DebouncedOperation.Update);
        }

        public async Task<Player> Update()
        {
            await RunUpdate();
            return Talo.CurrentPlayer;
        }

        private async Task<RejectedProp[]> RunUpdate()
        {
            Talo.IdentityCheck();

            var uri = new Uri($"{baseUrl}/{Talo.CurrentPlayer.id}");
            var content = JsonUtility.ToJson(Talo.CurrentPlayer);
            var json = await Call(uri, "PATCH", Prop.SanitiseJson(content));

            var res = JsonUtility.FromJson<PlayersUpdateResponse>(json);
            Talo.CurrentPlayer = res.player;
            Talo.CurrentAlias.WriteOfflineAlias();

            return res.rejectedProps ?? Array.Empty<RejectedProp>();
        }

        public async Task<Player> Merge(string playerId1, string playerId2, MergeOptions options = null)
        {
            options ??= new MergeOptions();

            // the store may not have the second player's claim key, so the game can supply it
            var player2ClaimKey = options.player2ClaimKey;
            if (string.IsNullOrEmpty(player2ClaimKey))
            {
                player2ClaimKey = Talo.ClaimKeys.GetPlayerKey(playerId2);
            }

            var uri = new Uri($"{baseUrl}/merge");
            string content = new PlayersMergeRequest(playerId1, playerId2, player2ClaimKey).ToJson();
            var json = await Call(uri, "POST", content);

            var res = JsonUtility.FromJson<PlayersUpdateResponse>(json);
            var player = res.player;

            HandleMergedClaimKeys(player, playerId2, player2ClaimKey);

            if (!string.IsNullOrEmpty(options.postMergeIdentityService))
            {
                var alias = player.GetAlias(options.postMergeIdentityService);
                if (alias != null)
                {
                    await Identify(alias.service, alias.identifier);
                }
            }

            return player;
        }

        private void HandleMergedClaimKeys(Player player, string playerId2, string player2ClaimKey)
        {
            // a surviving player with no claim key takes the merged player's key
            var mergedPlayerHasClaimKey = !string.IsNullOrEmpty(player2ClaimKey);
            var survivorHasNoClaimKey = string.IsNullOrEmpty(Talo.ClaimKeys.GetPlayerKey(player.id));
            if (mergedPlayerHasClaimKey && survivorHasNoClaimKey)
            {
                Talo.ClaimKeys.SaveKey(player.id, player2ClaimKey);
            }

            // the merged aliases belong to the surviving player now, so their keys follow
            var survivorKey = Talo.ClaimKeys.GetPlayerKey(player.id);
            foreach (var alias in player.aliases ?? Array.Empty<PlayerAlias>())
            {
                Talo.ClaimKeys.SaveKey(player.id, survivorKey, alias.service, alias.identifier);
            }

            Talo.ClaimKeys.ForgetPlayer(playerId2);
        }

        public async Task<Player> Find(string playerId)
        {
            var uri = new Uri($"{baseUrl}/{playerId}");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<PlayersFindResponse>(json);
            return res.player;
        }

        private async Task<PlayerAlias> IdentifyOffline(string service, string identifier)
        {
            PlayerAlias offlineAlias;
            try
            {
                offlineAlias = PlayerAlias.GetOfflineAlias();
            }
            catch
            {
                PlayerAlias.DeleteOfflineAlias();
                OnIdentificationFailed?.Invoke(new IdentifyException());
                throw new Exception("Failed to parse offline player alias");
            }

            if (offlineAlias != null && offlineAlias.MatchesIdentifyRequest(service, identifier))
            {
                return await HandleIdentifySuccess(offlineAlias);
            }

            OnIdentificationFailed?.Invoke(new IdentifyException());
            throw new Exception("No offline player alias found");
        }

        public async Task ClearIdentity()
        {
            if (await Talo.PlayerAuth.SessionManager.ClearSession())
            {
                OnIdentityCleared?.Invoke();
            }
        }

        public async Task<PlayersSearchResponse> Search(string query)
        {
            var encodedQuery = Uri.EscapeDataString(query.Trim());
            var uri = new Uri($"{baseUrl}/search?query={encodedQuery}");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<PlayersSearchResponse>(json);
            return res;
        }

        public async Task<string> CreateSocketToken()
        {
            Talo.IdentityCheck();

            var uri = new Uri($"{baseUrl}/socket-token");

            try
            {
                var json = await Call(uri, "POST");
                var res = JsonUtility.FromJson<PlayersSocketTokenResponse>(json);
                return res.socketToken;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to create socket token: {ex.Message}");
                return "";
            }
        }

        public class PlayerUpdateResult
        {
            public bool Success { get; }
            public RejectedProp[] RejectedProps { get; }

            public PlayerUpdateResult(bool success, RejectedProp[] rejectedProps = null)
            {
                Success = success;
                RejectedProps = rejectedProps ?? Array.Empty<RejectedProp>();
            }
        }
    }
}
