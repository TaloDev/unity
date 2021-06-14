using System;
using UnityEngine;
using UnityEditor;
using System.Net.Http;

namespace TaloGameServices {
    public class Talo {
        private static readonly HttpClient client = new HttpClient();
        private static EventsAPI _events;
        private static PlayersAPI _players;
        private static PlayerAlias _currentPlayer;

        public static PlayerAlias CurrentPlayer {
            get => _currentPlayer;
            set => _currentPlayer = value;
        }

        public static EventsAPI Events {
            get => _events;
        }

        public static PlayersAPI Players {
            get => _players;
        }

        static Talo() {
            string[] assets = AssetDatabase.FindAssets("t:TaloSettings");
            if (assets.Length == 0) {
                Debug.LogError("Talo settings asset not found. Create one using the Create menu > Talo > Settings Asset");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(assets[0]);
            TaloSettings settings = AssetDatabase.LoadAssetAtPath<TaloSettings>(path);

            _events = new EventsAPI(settings, client);
            _players = new PlayersAPI(settings, client);
        }

        public static void IdentityCheck() {
            if (CurrentPlayer == null) {
                throw new Exception("You need to identify a player using Talo.Identify() before doing this.");
            }
        }
    }
}
