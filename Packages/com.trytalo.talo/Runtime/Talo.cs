using System;
using UnityEngine;
using System.Net.Http;

namespace TaloGameServices
{
    public class Talo
    {
        private static readonly HttpClient client = new HttpClient();

        private static EventsAPI _events;
        private static PlayersAPI _players;
        private static LeaderboardsAPI _leaderboards;
        private static SavesAPI _saves;

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

        static Talo()
        {
            var settings = Resources.Load<TaloSettings>("Talo Settings");
            if (!settings)
            {
                Debug.LogError("'Talo Settings' asset not found in Resources folder. Create one using the Create menu > Talo > Settings Asset");
                return;
            }

            _events = new EventsAPI(settings, client);
            _players = new PlayersAPI(settings, client);
            _leaderboards = new LeaderboardsAPI(settings, client);
            _saves = new SavesAPI(settings, client);
        }

        public static void IdentityCheck()
        {
            if (CurrentAlias == null)
            {
                throw new Exception("You need to identify a player using Talo.Identify() before doing this.");
            }
        }
    }
}
