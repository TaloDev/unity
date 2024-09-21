using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class PlayerAuthAPI : BaseAPI
    {
        private SessionManager _sessionManager = new();

        public SessionManager SessionManager => _sessionManager;

        public PlayerAuthAPI() : base("v1/players/auth") {}

        public async Task Register(string identifier, string password, string email = "", bool verificationEnabled = false)
        {
            if (verificationEnabled && string.IsNullOrEmpty(email))
            {
                throw new Exception("Email is required when verification is enabled");
            }

            var uri = new Uri($"{baseUrl}/register");
            string content = JsonUtility.ToJson(new PlayerAuthRegisterRequest {
                identifier = identifier,
                password = password,
                email = email,
                verificationEnabled = verificationEnabled
            });
            var json = await Call(uri, "POST", content);

            var res = JsonUtility.FromJson<PlayerAuthSessionResponse>(json);
            _sessionManager.HandleSessionCreated(res);
        }

        public async Task<bool> Login(string identifier, string password)
        {
            var uri = new Uri($"{baseUrl}/login");
            string content = JsonUtility.ToJson(new PlayerAuthLoginRequest {
                identifier = identifier,
                password = password
            });

            var json = await Call(uri, "POST", content);

            var res = JsonUtility.FromJson<PlayerAuthLoginResponse>(json);

            if (res.verificationRequired)
            {
                _sessionManager.verificationAliasId = res.aliasId;
                return true;
            }
            else
            {
                _sessionManager.HandleSessionCreated(res);
                return false;
            }
        }

        public async Task Verify(string code)
        {
            var uri = new Uri($"{baseUrl}/verify");
            string content = JsonUtility.ToJson(new PlayerAuthVerifyRequest {
                aliasId = _sessionManager.verificationAliasId,
                code = code
            });
            var json = await Call(uri, "POST", content);

            var res = JsonUtility.FromJson<PlayerAuthSessionResponse>(json);
            _sessionManager.HandleSessionCreated(res);
        }

        public async Task Logout()
        {
            var uri = new Uri($"{baseUrl}/logout");
            await Call(uri, "POST");

            _sessionManager.ClearSession();
            Talo.CurrentAlias = null;
        }

        public async Task ChangePassword(string currentPassword, string newPassword)
        {
            var uri = new Uri($"{baseUrl}/change_password");
            string content = JsonUtility.ToJson(new PlayerAuthChangePasswordRequest {
                currentPassword = currentPassword,
                newPassword = newPassword
            });
            await Call(uri, "POST", content);
        }

        public async Task ChangeEmail(string currentPassword, string newEmail)
        {
            var uri = new Uri($"{baseUrl}/change_email");
            string content = JsonUtility.ToJson(new PlayerAuthChangeEmailRequest {
                currentPassword = currentPassword,
                newEmail = newEmail
            });
            await Call(uri, "POST", content);
        }

        public async Task ForgotPassword(string email)
        {
            var uri = new Uri($"{baseUrl}/forgot_password");
            string content = JsonUtility.ToJson(new PlayerAuthForgotPasswordRequest {
                email = email
            });
            await Call(uri, "POST", content);
        }

        public async Task ResetPassword(string code, string password)
        {
            var uri = new Uri($"{baseUrl}/reset_password");
            string content = JsonUtility.ToJson(new PlayerAuthResetPasswordRequest {
                code = code,
                password = password
            });
            await Call(uri, "POST", content);
        }

        public async Task ToggleVerification(string currentPassword, bool verificationEnabled, string email = "")
        {
            var uri = new Uri($"{baseUrl}/toggle_verification");
            string content = JsonUtility.ToJson(new PlayerAuthToggleVerificationRequest {
                currentPassword = currentPassword,
                verificationEnabled = verificationEnabled,
                email = email
            });
            await Call(uri, "PATCH", content);
        }

        public async Task DeleteAccount(string currentPassword)
        {
            var uri = new Uri($"{baseUrl}/");
            string content = JsonUtility.ToJson(new PlayerAuthDeleteAccountRequest {
                currentPassword = currentPassword
            });
            await Call(uri, "DELETE", content);

            _sessionManager.ClearSession();
            Talo.CurrentAlias = null;
        }
    }
}
