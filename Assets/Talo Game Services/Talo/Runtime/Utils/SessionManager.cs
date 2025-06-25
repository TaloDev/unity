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
            Talo.Socket.SetSocketToken(res.socketToken);
            Talo.Players.InvokeIdentifiedEvent();
            SaveSession(res.sessionToken);
        }

        private void SaveSession(string sessionToken)
        {
            PlayerPrefs.SetString("TaloSessionToken", sessionToken);
            PlayerPrefs.SetString("TaloSessionIdentifier", Talo.CurrentAlias.identifier);
        }

        public async Task ClearSession()
        {
            Talo.CurrentAlias = null;
            PlayerPrefs.DeleteKey("TaloSessionToken");
            await Talo.Socket.ResetConnection();
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
    }
}
