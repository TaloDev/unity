using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class SessionManager
    {
        public int verificationAliasId;

        private string _sessionToken;

        public void HandleSessionCreated(PlayerAuthSessionResponse res)
        {
            Talo.CurrentAlias = res.alias;
            SaveSession(res.sessionToken, res.refreshToken);
            Talo.Players.InvokeIdentifiedEvent();
            Talo.Socket.SetSocketToken(res.socketToken);
        }

        public void HandleSessionRefreshed(string sessionToken, string refreshToken)
        {
            SaveSession(sessionToken, refreshToken);
        }

        private void SetIdentifierPlayerPref()
        {
            if (Talo.CurrentAlias == null)
            {
                return;
            }

            PlayerPrefs.SetString("TaloSessionIdentifier", Talo.CurrentAlias.identifier);
        }

        private void SaveSession(string sessionToken, string refreshToken)
        {
            _sessionToken = sessionToken;
            PlayerPrefs.SetString("TaloRefreshToken", refreshToken);
            SetIdentifierPlayerPref();
        }

        public async Task<bool> ClearSession(bool resetSocket = true)
        {
            if (!Talo.HasIdentity())
            {
                return false;
            }

            _sessionToken = null;
            Talo.CurrentAlias = null;
            PlayerAlias.DeleteOfflineAlias();

            PlayerPrefs.DeleteKey("TaloRefreshToken");

            Talo.Events.ClearQueue();
            Talo.Continuity.ClearRequests();

            if (resetSocket)
            {
                try
                {
                    await Talo.Socket.ResetConnection();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error resetting socket: {ex.Message}");
                }
            }

            return true;
        }

        public string GetSessionToken()
        {
            return _sessionToken;
        }

        public string GetRefreshToken()
        {
            return PlayerPrefs.GetString("TaloRefreshToken");
        }

        public string GetSessionIdentifier()
        {
            return PlayerPrefs.GetString("TaloSessionIdentifier");
        }

        public async Task<bool> CheckForSession()
        {
            if (!string.IsNullOrEmpty(_sessionToken))
            {
                return true;
            }

            if (Talo.IsOffline())
            {
                return PlayerAlias.HasOfflineAlias();
            }

            if (!string.IsNullOrEmpty(GetRefreshToken()))
            {
                try
                {
                    await Talo.PlayerAuth.Refresh();
                }
                catch
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        private void SetNewAlias(PlayerAlias alias)
        {
            Talo.CurrentAlias = alias;
            alias.WriteOfflineAlias();
        }

        public void HandleIdentifierUpdated(PlayerAuthChangeIdentifierResponse res)
        {
            SetNewAlias(res.alias);
            SetIdentifierPlayerPref();
        }

        public async Task HandleAccountMigrated(PlayerAuthMigrateAccountResponse res)
        {
            await ClearSession(false);
            SetNewAlias(res.alias);
            Talo.Players.InvokeIdentifiedEvent();
        }
    }
}
