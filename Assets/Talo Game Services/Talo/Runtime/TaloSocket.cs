using UnityEngine;
using MikeSchweitzer.WebSocket;
using System.Threading.Tasks;
using System;

namespace TaloGameServices
{
    public class TaloSocket : MonoBehaviour
    {
        public event Action<SocketResponse> OnMessageReceived;
        public event Action OnConnectionClosed;
        public event Action<SocketError> OnErrorReceived;

        private WebSocketConnection socket;
        private string tempSocketToken;
        private bool socketAuthenticated;
        private bool identified;

        private void Awake()
        {
            socket = gameObject.AddComponent<WebSocketConnection>();
            socket.MessageReceived += HandleMessage;
            socket.StateChanged += HandleStateChange;
        }

        private async void Start()
        {
            if (Talo.Settings.autoConnectSocket)
            {
                await OpenConnection();
            }
        }

        private void OnDestroy()
        {
            socket.MessageReceived -= HandleMessage;
            socket.StateChanged -= HandleStateChange;
        }

        private void HandleMessage(WebSocketConnection connection, WebSocketMessage wsm)
        {
            var response = new SocketResponse(wsm.String);
            var res = response.GetResponseType();
            OnMessageReceived?.Invoke(response);

            if (Talo.Settings.logResponses && Debug.isDebugBuild)
            {
                Debug.Log($"--> WSS {res} {response.GetJsonData()}");
            }

            switch (res)
            {
                case "v1.connected":
                    socketAuthenticated = true;
                    if (!identified && !string.IsNullOrEmpty(tempSocketToken))
                    {
                        IdentifyPlayer();
                    }
                    break;
                case "v1.players.identify.success":
                    identified = true;
                    tempSocketToken = "";
                    break;
                case "v1.error":
                    OnErrorReceived?.Invoke(response.GetData<SocketError>());
                    break;
            }
        }

        private void HandleStateChange(WebSocketConnection connection, WebSocketState oldState, WebSocketState newState)
        {
            if (newState == WebSocketState.Disconnected)
            {
                OnConnectionClosed?.Invoke();
            }
        }

        public async Task OpenConnection()
        {
            var ticket = await Talo.SocketTickets.CreateTicket();
            socket.Connect($"{Talo.Settings.socketUrl}/?ticket={ticket}");
        }

        public void CloseConnection()
        {
            socket.Disconnect();
        }

        public void Send<T>(SocketRequest<T> request)
        {
            var data = JsonUtility.ToJson(request);

            if (Talo.Settings.logRequests && Debug.isDebugBuild)
            {
                Debug.Log($"<-- WSS {request.req} {data}");
            }
            socket.AddOutgoingMessage(data);
        }

        private void IdentifyPlayer()
        {
            if (!socketAuthenticated)
            {
                return;
            }

            var payload = new IdentifyPlayer
            {
                playerAliasId = Talo.CurrentAlias.id,
                socketToken = tempSocketToken
            };

            var sessionToken = Talo.PlayerAuth.SessionManager.GetSessionToken();
            if (!string.IsNullOrEmpty(sessionToken))
            {
                payload.sessionToken = sessionToken;
            }

            Send(new SocketRequest<IdentifyPlayer>("v1.players.identify", payload));
        }

        public void SetSocketToken(string token)
        {
            tempSocketToken = token;
            if (!identified && socketAuthenticated)
            {
                IdentifyPlayer();
            }
        }

        public async Task ResetConnection()
        {
            if (!identified)
            {
                return;
            }

            CloseConnection();
            socketAuthenticated = false;
            identified = false;
            await OpenConnection();
        }
    }
}
