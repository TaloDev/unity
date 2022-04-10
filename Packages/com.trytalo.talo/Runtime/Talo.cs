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

        static Talo()
        {
            var settings = Resources.Load<TaloSettings>("Talo Settings");
            if (!settings)
            {
                Debug.LogError("'Talo Settings' asset not found in Resources folder. Create one using the Create menu > Talo > Settings Asset");
                return;
            }

            var manager = new GameObject("Talo Manager").AddComponent<TaloManager>();
            manager.settings = settings;

            _events = new EventsAPI(manager);
            _players = new PlayersAPI(manager);
            _leaderboards = new LeaderboardsAPI(manager);
            _saves = new SavesAPI(manager);
            _stats = new StatsAPI(manager);
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
