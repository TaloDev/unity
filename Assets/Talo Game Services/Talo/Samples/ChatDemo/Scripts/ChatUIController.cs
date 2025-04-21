using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace TaloGameServices.Sample.ChatDemo
{
    public class ChatUIController : MonoBehaviour
    {
        public string playerUsername;
        private int activeChannelId;

        private List<string> messages = new List<string>();
        private VisualElement root;
        private ListView messagesList;
        private VisualElement channelsList;
        private TextField messageField;
        private Button sendButton;
        private TextField channelNameField;
        private Button createButton;

        private async void Start()
        {
            root = GetComponent<UIDocument>().rootVisualElement;

            InitMessagesList();
            messageField = root.Q<TextField>("message");
            sendButton = root.Q<Button>("post-btn");
            sendButton.clicked += OnSendClick;

            Talo.Channels.OnMessageReceived += OnMessageReceived;
            Talo.PlayerPresence.OnPresenceChanged += OnPresenceChanged;

            if (string.IsNullOrEmpty(playerUsername))
            {
                AddMessage("[SYSTEM] No player username set, please set one in the UI game object");
                return;
            }

            await Talo.Players.Identify("username", playerUsername);
            AddMessage("[SYSTEM] Identified as " + playerUsername);

            InitChannelsList();

            channelNameField = root.Q<TextField>("channel-name");
            createButton = root.Q<Button>("create-btn");
            createButton.clicked += OnCreateChannelClick;
        }

        private void InitMessagesList()
        {
            messagesList = root.Q<ListView>("list");

            messagesList.makeItem = () =>
            {
                var label = new Label();
                label.style.color = new StyleColor(Color.white);
                label.style.fontSize = 22;

                return label;
            };

            messagesList.bindItem = (e, i) =>
            {
                e.Q<Label>().text = messages[i];
            };

            messagesList.itemsSource = messages;
        }

        private void AddChannelToList(Channel channel)
        {
            var button = new Button();
            button.text = channel.name;
            button.style.fontSize = 16;
            button.style.height = 20;
            button.clicked += async () => {
                await Talo.Channels.Join(channel.id);
                activeChannelId = channel.id;
                AddMessage("[SYSTEM] Switched to channel " + channel.name);
            };
            channelsList.Add(button);
        }

        private async void InitChannelsList()
        {
            var res = await Talo.Channels.GetChannels(0);

            channelsList = root.Q<VisualElement>("channels");
            foreach (var channel in res.channels)
            {
                AddChannelToList(channel);
            }
        }

        private void OnSendClick()
        {
            if (Talo.CurrentPlayer == null)
            {
                AddMessage("[SYSTEM] No player username set, please set one in the UI game object");
                return;
            }

            if (activeChannelId < 1)
            {
                AddMessage("[SYSTEM] No active channel set, please join a channel");
                return;
            }

            var message = messageField.text;
            if (string.IsNullOrEmpty(message))
            {
                AddMessage("[SYSTEM] Message cannot be empty");
                return;
            }

            Talo.Channels.SendMessage(activeChannelId, message);
            messageField.value = "";
        }

        private async void OnCreateChannelClick()
        {
            if (Talo.CurrentPlayer == null)
            {
                AddMessage("[SYSTEM] No player username set, please set one in the UI game object");
                return;
            }

            var channelName = channelNameField.text;
            if (string.IsNullOrEmpty(channelName))
            {
                AddMessage("[SYSTEM] Channel name cannot be empty");
                return;
            }

            var channel = await Talo.Channels.Create(channelName);
            AddChannelToList(channel);
            channelNameField.value = "";
            activeChannelId = channel.id;

            AddMessage("[SYSTEM] Created and joined channel " + channel.name);
        }

        private void AddMessage(string message)
        {
            messages.Add(message);
            messagesList.Rebuild();
        }

        private void OnMessageReceived(Channel channel, PlayerAlias playerAlias, string message)
        {
            if (channel.id == activeChannelId)
            {
                AddMessage($"[{channel.name}] {playerAlias.identifier}: {message}");
            }
        }

        private void OnPresenceChanged(PlayerPresence presence, bool onlineChanged, bool customStatusChanged)
        {
            if (onlineChanged)
            {
                AddMessage($"[SYSTEM] {presence.playerAlias.identifier} is now {(presence.online ? "online" : "offline")}");
            }
        }
    }
}
