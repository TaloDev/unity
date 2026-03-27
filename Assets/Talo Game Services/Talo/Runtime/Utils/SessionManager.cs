using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class SessionManager
    {
        public int verificationAliasId;

        public void HandleSessionCreated(PlayerAuthSessionResponse res)
        {
            Talo.CurrentAlias = res.alias;
            SaveSession(res.sessionToken);
            Talo.Players.InvokeIdentifiedEvent();
            Talo.Socket.SetSocketToken(res.socketToken);
        }

        private void SetIdentifierPlayerPref()
        {
            PlayerPrefs.SetString("TaloSessionIdentifier", Talo.CurrentAlias.identifier);
        }

        private void SaveSession(string sessionToken)
        {
            PlayerPrefs.SetString("TaloSessionToken", sessionToken);
            SetIdentifierPlayerPref();
        }

        public async Task ClearSession(bool resetSocket = true)
        {
            Talo.CurrentAlias = null;
            PlayerPrefs.DeleteKey("TaloSessionToken");
            if (resetSocket)
            {
                await Talo.Socket.ResetConnection();
            }
        }

        public string GetSessionToken()
        {
            return PlayerPrefs.GetString("TaloSessionToken");
        }

        public string GetSessionIdentifier()
        {
            return PlayerPrefs.GetString("TaloSessionIdentifier");
        }

        public bool CheckForSession()
        {
            return !string.IsNullOrEmpty(GetSessionToken());
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
