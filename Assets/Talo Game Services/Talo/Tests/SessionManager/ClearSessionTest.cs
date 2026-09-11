using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TaloGameServices.Test
{
    internal class ClearSessionTest
    {
        [OneTimeSetUp]
        public void Setup()
        {
            var tm = new GameObject().AddComponent<TaloManager>();
            tm.settings = ScriptableObject.CreateInstance<TaloSettings>();
            tm.settings.autoConnectSocket = false;
            tm.settings.autoStartSession = false;

            Talo.CurrentAlias = null;
            PlayerPrefs.DeleteAll();
        }

        [UnityTest]
        public IEnumerator ClearSession_WithoutIdentity_DeletesStoredToken()
        {
            Talo.CurrentAlias = null;
            PlayerPrefs.SetString("TaloRefreshToken", "stale-token");

            // TestMode makes HasIdentity() always return true, which would mask
            // the no-identity path this test exercises
            TestModeFlag.IsEnabled = false;
            try
            {
                var task = Talo.PlayerAuth.SessionManager.ClearSession(false);
                while (!task.IsCompleted)
                {
                    yield return null;
                }

                Assert.IsFalse(task.Result);
                Assert.IsEmpty(PlayerPrefs.GetString("TaloRefreshToken"));
            }
            finally
            {
                TestModeFlag.IsEnabled = true;
            }
        }

        [UnityTest]
        public IEnumerator ClearSession_WithIdentity_ReturnsTrue()
        {
            Talo.CurrentAlias = new PlayerAlias() {
                player = new Player() {
                    id = "uuid"
                }
            };
            PlayerPrefs.SetString("TaloRefreshToken", "some-token");

            var task = Talo.PlayerAuth.SessionManager.ClearSession(false);
            while (!task.IsCompleted)
            {
                yield return null;
            }

            Assert.IsTrue(task.Result);
            Assert.IsEmpty(PlayerPrefs.GetString("TaloRefreshToken"));
        }
    }
}
