using UnityEngine;
using UnityEngine.UIElements;
using TaloGameServices;
using System.Threading.Tasks;
using System;
using System.Linq;

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

        await Talo.Players.Identify("temp", Guid.NewGuid().ToString());
        await Talo.Channels.Join(demoChannel.id);

        if (!string.IsNullOrEmpty(propKey))
        {
            propKeyField.value = propKey;
            var existingProp = await Talo.Channels.GetStorageProp(demoChannel.id, propKey);
            if (existingProp != null)
            {
                propValueField.value = existingProp.value;
                OnChannelStoragePropsUpdated(demoChannel, new ChannelStorageProp[] { existingProp }, Array.Empty<ChannelStorageProp>());
            }
        }
    }

    private void OnChannelStoragePropsUpdated(Channel channel, ChannelStorageProp[] upsertedProps, ChannelStorageProp[] deletedProps)
    {
        if (channel.id != demoChannel.id)
            return;

        foreach (var prop in upsertedProps)
        {
            if (prop.key == propKeyField.text)
            {
                propLiveValueLabel.text = $"{prop.key} live value is: {prop.value}";
                propUpdatedLabel.text = $"{prop.key} was last updated by {(prop.lastUpdatedBy.id == Talo.CurrentAlias.id ? "you" : prop.lastUpdatedBy.identifier)} at {prop.updatedAt}.";
            }
        }

        foreach (var prop in deletedProps)
        {
            if (prop.key == propKeyField.text)
            {
                propLiveValueLabel.text = $"{prop.key} live value is: (deleted)";
                propUpdatedLabel.text = $"{prop.key} was deleted by {(prop.lastUpdatedBy.id == Talo.CurrentAlias.id ? "you" : prop.lastUpdatedBy.identifier)} at {prop.updatedAt}.";
            }
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

        await Talo.Channels.SetStorageProps(demoChannel.id, (propKeyField.value, propValueField.value));
    }

    private async void OnDeleteButtonClicked()
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

        await Talo.Channels.SetStorageProps(demoChannel.id, (propKeyField.value, null));
    }
}
