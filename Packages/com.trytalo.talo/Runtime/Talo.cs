using System;
using UnityEngine;

namespace TaloGameServices
{
    public class Talo
    {
        private static EventsAPI _events;
        private static PlayersAPI _players;
        private static LeaderboardsAPI _leaderboards;
        private static SavesAPI _saves;
        private static StatsAPI _stats;
        private static GameConfigAPI _gameConfig;

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

        static Talo()
        {
            var settings = Resources.Load<TaloSettings>("Talo Settings");
            if (!settings)
            {
                Debug.LogError("A 'Talo Settings' asset was not found in the Resources folder. Create one using the Create menu > Talo > Settings Asset");
                return;
            }

            var manager = new GameObject("Talo Manager").AddComponent<TaloManager>();
            manager.settings = settings;

            _events = new EventsAPI(manager);
            _players = new PlayersAPI(manager);
            _leaderboards = new LeaderboardsAPI(manager);
            _saves = new SavesAPI(manager);
            _stats = new StatsAPI(manager);
            _gameConfig = new GameConfigAPI(manager);
        }

        public static bool HasIdentity()
        {
            return CurrentAlias != null;
        }

        public static void IdentityCheck()
        {
            if (!HasIdentity())
            {
                throw new Exception("You need to identify a player using Talo.Identify() before doing this.");
            }
        }
    }
}
