using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;

namespace TaloGameServices.Test
{
    internal class IdentityClearedEventMock
    {
        public bool identityCleared = false;

        public void Invoke()
        {
            identityCleared = true;
        }
    }

    internal class ClearIdentityTest
    {
        [OneTimeSetUp]
        public void Setup()
        {
            var tm = new GameObject().AddComponent<TaloManager>();
            tm.settings = ScriptableObject.CreateInstance<TaloSettings>();
            tm.settings.autoConnectSocket = false;

            Talo.CurrentAlias = new PlayerAlias() {
                player = new Player() {
                    id = "uuid"
                }
            };
        }

        [UnityTest]
        public IEnumerator ClearIdentity_ShouldClearAliasData()
        {
            var eventMock = new IdentityClearedEventMock();
            Talo.Players.OnIdentityCleared += eventMock.Invoke;

            yield return Talo.Events.Track("test-event");
            Assert.IsNotEmpty(Talo.Events.queue);

            _ = Talo.Players.ClearIdentity();
            Assert.IsNull(Talo.CurrentAlias);
            Assert.IsTrue(eventMock.identityCleared);
            Assert.IsEmpty(Talo.Events.queue);
            Assert.IsEmpty(Talo.Events.eventsToFlush);
            Assert.IsFalse(Talo.Continuity.HasRequests());

            Talo.Players.OnIdentityCleared -= eventMock.Invoke;
        }
    }
}