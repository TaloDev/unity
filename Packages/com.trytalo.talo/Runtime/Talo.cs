using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TaloGameServices
{
    public class Talo
    {
        private static bool _testMode;

        internal static bool TestMode => _testMode;

        internal static EventsAPI _events;
        internal static PlayersAPI _players;
        internal static LeaderboardsAPI _leaderboards;
        internal static SavesAPI _saves;
        internal static StatsAPI _stats;
        internal static GameConfigAPI _gameConfig;
        internal static FeedbackAPI _feedback;
        internal static PlayerAuthAPI _playerAuth;

        private static PlayerAlias _currentAlias;

        public static PlayerAlias CurrentAlias
        {
            get => _currentAlias;
            set => _currentAlias = value;
        }

        public static Player CurrentPlayer
        {
            get
            {
                IdentityCheck();
                return _currentAlias.player;
            }
            set => _currentAlias.player = value;
        }

        private static LiveConfig _liveConfig;

        public static LiveConfig LiveConfig
        {
            get {
                if (_liveConfig == null)
                {
                    throw new Exception("Live config needs to be inited first - use Talo.GameConfig.Get() to fetch it");
                }
                return _liveConfig;
            }
            set => _liveConfig = value;
        }

        public static EventsAPI Events
        {
            get => _events;
        }

        public static PlayersAPI Players
        {
            get => _players;
        }

        public static LeaderboardsAPI Leaderboards
        {
            get => _leaderboards;
        }

        public static SavesAPI Saves
        {
            get => _saves;
        }

        public static StatsAPI Stats
        {
            get => _stats;
        }

        public static GameConfigAPI GameConfig
        {
            get => _gameConfig;
        }

        public static FeedbackAPI Feedback
        {
            get => _feedback;
        }

        public static PlayerAuthAPI PlayerAuth
        {
            get => _playerAuth;
        }

        static Talo()
        {
            TaloManager tm;

            if (!CheckTestMode())
            {
                var settings = Resources.Load<TaloSettings>("Talo Settings");
                if (!settings)
                {
                    Debug.LogError("A 'Talo Settings' asset was not found in the Resources folder. Create one using the Create menu > Talo > Settings Asset");
                    return;
                }

                tm = new GameObject("Talo Manager").AddComponent<TaloManager>();
                tm.settings = settings;
            }
            else
            {
                tm = UnityEngine.Object.FindObjectOfType<TaloManager>();
            }

            _events = new EventsAPI(tm);
            _players = new PlayersAPI(tm);
            _leaderboards = new LeaderboardsAPI(tm);
            _saves = new SavesAPI(tm);
            _stats = new StatsAPI(tm);
            _gameConfig = new GameConfigAPI(tm);
            _feedback = new FeedbackAPI(tm);
            _playerAuth = new PlayerAuthAPI(tm);
        }

        public static bool HasIdentity()
        {
            return _testMode || CurrentAlias != null;
        }

        public static void IdentityCheck()
        {
            if (!HasIdentity())
            {
                throw new Exception("You need to identify a player using Talo.Players.Identify() before doing this.");
            }
        }

        public static bool IsOffline()
        {
            if (TestMode) return RequestMock.Offline;
            return Application.internetReachability == NetworkReachability.NotReachable;
        }

        internal static bool CheckTestMode()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault((assembly) => assembly.FullName.ToLowerInvariant().StartsWith("nunit.framework"));

            if (assembly != null)
            {
                try
                {
                    _testMode = TestContext.CurrentContext.Test.ID != null;
                    return _testMode;
                }
                catch
                {
                    return false;
                }
            }

            return _testMode;
        }
    }
}
