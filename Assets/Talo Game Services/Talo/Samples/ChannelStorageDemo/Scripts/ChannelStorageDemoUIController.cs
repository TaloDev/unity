using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace TaloGameServices.Sample.ChannelStorageDemo
{
    public class ChannelStorageDemoUIController : MonoBehaviour
    {
        public string propKey;

        private VisualElement root;
        private TextField propKeyField;
        private TextField propValueField;
        private Label propLiveValueLabel;
        private Label propUpdatedLabel;
        private Button upsertButton;
        private Button deleteButton;

        private Channel demoChannel;

        async void Start()
        {
            Talo.Channels.OnChannelStoragePropsUpdated += OnChannelStoragePropsUpdated;
            Talo.Channels.OnChannelStoragePropsFailedToSet += OnChannelStoragePropsFailedToSet;

            SetupUI();
            await SetupDemoChannel();
        }

        private void SetupUI()
        {
            root = GetComponent<UIDocument>().rootVisualElement;

            propKeyField = root.Q<TextField>("prop-key-field");
            propValueField = root.Q<TextField>("prop-value-field");
            propLiveValueLabel = root.Q<Label>("prop-live-value-label");
            propUpdatedLabel = root.Q<Label>("prop-updated-label");
            upsertButton = root.Q<Button>("upsert-btn");
            deleteButton = root.Q<Button>("delete-btn");

            upsertButton.clicked += OnUpsertButtonClicked;
            deleteButton.clicked += OnDeleteButtonClicked;
        }

        private async Task SetupDemoChannel()
        {
            propLiveValueLabel.text = "Set a prop to see live updates";
            propUpdatedLabel.text = "No prop key set";
            await Talo.Players.Identify("temp_username", Guid.NewGuid().ToString());

            var res = await Talo.Channels.GetChannels(new GetChannelsOptions() { propKey = "channel-storage-demo" });
            if (res.channels.Count() > 0)
            {
                demoChannel = res.channels[0];
            }
            else
            {
                var createOptions = new CreateChannelOptions()
                {
                    name = "Channel Storage Demo",
                    props = new (string, string)[]
                    {
                    ( "channel-storage-demo", "true" )
                    }
                };
                demoChannel = await Talo.Channels.Create(createOptions);
            }

            await Talo.Channels.Join(demoChannel.id);

            if (!string.IsNullOrEmpty(propKey))
            {
                propKeyField.value = propKey;
                if (propKey.EndsWith("[]"))
                {
                    var existingProps = await Talo.Channels.GetStoragePropArray(demoChannel.id, propKey);
                    if (existingProps.Length > 0)
                    {
                        propValueField.value = string.Join(", ", existingProps.Select((p) => p.value));
                        OnChannelStoragePropsUpdated(demoChannel, existingProps, Array.Empty<ChannelStorageProp>());
                    }
                }
                else
                {
                    var existingProp = await Talo.Channels.GetStorageProp(demoChannel.id, propKey);
                    if (existingProp != null)
                    {
                        propValueField.value = existingProp.value;
                        OnChannelStoragePropsUpdated(demoChannel, new ChannelStorageProp[] { existingProp }, Array.Empty<ChannelStorageProp>());
                    }
                }
            }
        }

        private async void OnChannelStoragePropsUpdated(Channel channel, ChannelStorageProp[] upsertedProps, ChannelStorageProp[] deletedProps)
        {
            if (channel.id != demoChannel.id)
            {
                return;
            }

            var currentKey = propKeyField.value;
            var isArray = currentKey.EndsWith("[]");

            var matchingUpserted = upsertedProps.Where((p) => p.key == currentKey).ToArray();
            if (matchingUpserted.Length > 0)
            {
                var lastProp = matchingUpserted.Last();
                if (isArray)
                {
                    var current = await Talo.Channels.GetStoragePropArray(demoChannel.id, currentKey);
                    propLiveValueLabel.text = $"{currentKey} live values are: [{string.Join(", ", current.Select((p) => p.value))}]";
                }
                else
                {
                    propLiveValueLabel.text = $"{currentKey} live value is: {lastProp.value}";
                }
                propUpdatedLabel.text = $"{currentKey} was last updated by {(lastProp.lastUpdatedBy.id == Talo.CurrentAlias.id ? "you" : lastProp.lastUpdatedBy.identifier)} at {lastProp.updatedAt}.";
            }

            var matchingDeleted = deletedProps.Where((p) => p.key == currentKey).ToArray();
            if (matchingDeleted.Length > 0)
            {
                var lastProp = matchingDeleted.Last();
                if (isArray)
                {
                    var remaining = await Talo.Channels.GetStoragePropArray(demoChannel.id, currentKey);
                    propLiveValueLabel.text = remaining.Length == 0
                        ? $"{currentKey} live values are: (deleted)"
                        : $"{currentKey} live values are: [{string.Join(", ", remaining.Select((p) => p.value))}]";
                }
                else
                {
                    propLiveValueLabel.text = $"{currentKey} live value is: (deleted)";
                }
                propUpdatedLabel.text = $"{currentKey} was deleted by {(lastProp.lastUpdatedBy.id == Talo.CurrentAlias.id ? "you" : lastProp.lastUpdatedBy.identifier)} at {lastProp.updatedAt}.";
            }
        }

        private void OnChannelStoragePropsFailedToSet(Channel channel, ChannelStoragePropError[] errors)
        {
            foreach (var prop in errors)
            {
                Debug.Log($"{prop.key}: {prop.message} ({prop.error})");
            }
        }

        private async void OnUpsertButtonClicked()
        {
            if (string.IsNullOrEmpty(propKeyField.value))
            {
                propUpdatedLabel.text = "No prop key set";
                return;
            }
            if (string.IsNullOrEmpty(propValueField.value))
            {
                propUpdatedLabel.text = "No prop value set";
                return;
            }

            var key = propKeyField.value;
            if (key.EndsWith("[]"))
            {
                var items = propValueField.value.Split(',')
                    .Select((s) => s.Trim())
                    .Where((s) => !string.IsNullOrEmpty(s))
                    .ToArray();
                await Talo.Channels.SetStoragePropArray(demoChannel.id, key, items);
            }
            else
            {
                await Talo.Channels.SetStorageProps(demoChannel.id, (key, propValueField.value));
            }
        }

        private async void OnDeleteButtonClicked()
        {
            if (string.IsNullOrEmpty(propKeyField.value))
            {
                propUpdatedLabel.text = "No prop key set";
                return;
            }

            await Talo.Channels.SetStorageProps(demoChannel.id, (propKeyField.value, null));
        }
    }
}
