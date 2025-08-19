using System;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

namespace TaloGameServices
{
    public class PlayersAPI : BaseAPI
    {
        public event Action<Player> OnIdentified;
        public event Action OnIdentificationStarted;
        public event Action OnIdentificationFailed;

        private readonly string _offlineDataPath = Application.persistentDataPath + "/ta.bin";

        public PlayersAPI() : base("v1/players") { }

        public void InvokeIdentifiedEvent()
        {
            OnIdentified?.Invoke(Talo.CurrentPlayer);
        }

        private async Task<Player> HandleIdentifySuccess(PlayerAlias alias, string socketToken = "")
        {
            if (!Talo.IsOffline())
            {
                await Talo.Socket.ResetConnection();
            }

            Talo.CurrentAlias = alias;
            if (!string.IsNullOrEmpty(socketToken))
            {
                Talo.Socket.SetSocketToken(socketToken);
            }

            WriteOfflineAlias();
            InvokeIdentifiedEvent();

            return alias.player;
        }

        public async Task<Player> Identify(string service, string identifier)
        {
            OnIdentificationStarted?.Invoke();

            if (Talo.IsOffline())
            {
                var offlineAlias = GetOfflineAlias();
                if (offlineAlias != null)
                {
                    return await HandleIdentifySuccess(offlineAlias);
                }
                else
                {
                    OnIdentificationFailed?.Invoke();
                    throw new Exception("No offline player alias found.");
                }
            }

            var uri = new Uri($"{baseUrl}/identify?service={service}&identifier={identifier}");

            try
            {
                var json = await Call(uri, "GET");

                var res = JsonUtility.FromJson<PlayersIdentifyResponse>(json);
                return await HandleIdentifySuccess(res.alias, res.socketToken);
            }
            catch
            {
                await Talo.PlayerAuth.SessionManager.ClearSession();
                OnIdentificationFailed?.Invoke();
                throw;
            }
        }

        public async Task<Player> IdentifySteam(string ticket, string identity = "")
        {
            if (string.IsNullOrEmpty(identity))
            {
                await Identify("steam", ticket);
            }
            else
            {
                await Identify("steam", $"{identity}:{ticket}");
            }

            return Talo.CurrentPlayer;
        }

        public async Task<Player> Update()
        {
            var uri = new Uri($"{baseUrl}/{Talo.CurrentPlayer.id}");
            var content = JsonUtility.ToJson(Talo.CurrentPlayer);
            var json = await Call(uri, "PATCH", Prop.SanitiseJson(content));

            var res = JsonUtility.FromJson<PlayersUpdateResponse>(json);
            Talo.CurrentPlayer = res.player;
            WriteOfflineAlias();

            return Talo.CurrentPlayer;
        }

        public async Task<Player> Merge(string playerId1, string playerId2)
        {
            var uri = new Uri($"{baseUrl}/merge");
            string content = JsonUtility.ToJson(new PlayersMergeRequest(playerId1, playerId2));
            var json = await Call(uri, "POST", content);

            var res = JsonUtility.FromJson<PlayersUpdateResponse>(json);
            return res.player;
        }

        public async Task<Player> Find(string playerId)
        {
            var uri = new Uri($"{baseUrl}/{playerId}");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<PlayersFindResponse>(json);
            return res.player;
        }

        private void WriteOfflineAlias()
        {
            var content = JsonUtility.ToJson(Talo.CurrentAlias);
            Talo.Crypto.WriteFileContent(_offlineDataPath, content);
        }

        private PlayerAlias GetOfflineAlias()
        {
            if (!File.Exists(_offlineDataPath)) return null;
            return JsonUtility.FromJson<PlayerAlias>(Talo.Crypto.ReadFileContent(_offlineDataPath));
        }
    }
}
