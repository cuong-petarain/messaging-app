using Nakama;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Nakama.TinyJson;
using PimDeWitte.UnityMainThreadDispatcher;
using UnityEngine.UI;
using DG.Tweening;
using System.Text;

public class PanelMessage : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform _activeChatsPanel;
    [SerializeField] private Button _buttonPanelChats;

    [Header("Prefabs")]
    [SerializeField] private NameplateCard _nameplateCardPrefab;
    [SerializeField] private GameObject _messagingComponentsPrefab;

    private List<NameplateCard> _chattingChannels = new List<NameplateCard>();
    List<string> joinedChannels = new List<string>();
    private string HISTORY_CHANNELS_STRING = "HistoryChannels";

    public static Action<string, string> OnSentInviteDirectMessage;
    public static Action<string, string, string> OnMessageSubmitted;
    public static Action<string> OnNameplateCardClicked;
    public static Action<NameplateCard> OnNameplateCardRemoved;

    private void OnEnable()
    {
        ClientObject.Instance.Socket.ReceivedNotification += HandleNotification;
        ClientObject.Instance.Socket.ReceivedChannelMessage += HandleIncomingMessages;

        OnSentInviteDirectMessage += InviteDirectMessage;
        OnMessageSubmitted += SendMessageToActiveChannel;
        OnNameplateCardClicked += ActivateChannel;
        OnNameplateCardRemoved += DeleteCardAndActivateNextOne;
    }

    private void OnDisable()
    {
        ClientObject.Instance.Socket.ReceivedNotification -= HandleNotification;
        ClientObject.Instance.Socket.ReceivedChannelMessage -= HandleIncomingMessages;

        OnSentInviteDirectMessage -= InviteDirectMessage;
        OnMessageSubmitted -= SendMessageToActiveChannel;
        OnNameplateCardClicked -= ActivateChannel;
        OnNameplateCardRemoved -= DeleteCardAndActivateNextOne;

        SaveChannelsToDisk();
    }

    private async void Start()
    {
        while (ClientObject.Instance.Socket == null || !ClientObject.Instance.Socket.IsConnected || ClientObject.Instance.ThisUser == null)
        {
            await Task.Yield();
        }
        HISTORY_CHANNELS_STRING += ClientObject.Instance.ThisUser.Username;
        LoadChannelsFromPlayerPrefs();
    }

    private async void LoadChannelsFromPlayerPrefs()
    {
        joinedChannels = GetChannels();
        if (joinedChannels.Count == 0)
            return;

        foreach (string channel in joinedChannels)
        {
            if (!string.IsNullOrEmpty(channel))
            {
                string[] channelDetails = channel.Split(',');
                _ = await ClientObject.Instance.Socket.JoinChatAsync(channelDetails[1], ChannelType.Room, true, false);
                StartCoroutine(CreateChannel(channelDetails[0], channelDetails[1], channelDetails[2]));
            }
        }
    }

    private async void InviteDirectMessage(string toUserId, string toUserName)
    {
        _buttonPanelChats.onClick.Invoke();
        string roomName = CheckIfRoomExist(ClientObject.Instance.ThisUser.Username, toUserName);
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = $"{ClientObject.Instance.ThisUser.Username}.{toUserName}";
            IChannel tmpChannel = await ClientObject.Instance.Socket.JoinChatAsync(toUserId, ChannelType.DirectMessage);  // to send notification to target user
            IChannel channel = await ClientObject.Instance.Socket.JoinChatAsync(roomName, ChannelType.Room, true, false);
            IApiUsers users = await ClientObject.Instance.Client.GetUsersAsync(ClientObject.Instance.Session, new string[] { toUserId });

            var usersList = users.Users.ToList();
            if (usersList.Count > 0)
            {
                StartCoroutine(CreateChannel(channel.Id, channel.RoomName, usersList[0].DisplayName));
                AddChannelToJoined(channel.Id, channel.RoomName, usersList[0].DisplayName);
            }
            await ClientObject.Instance.Socket.LeaveChatAsync(tmpChannel.Id);  // leave DirectMessage channel
        }
        else
        {
            ActivateChannel(roomName);
        }
    }

    private async void AcceptDirectMessage(string requestorUsername)
    {
        try
        {
            string roomName = CheckIfRoomExist(requestorUsername, ClientObject.Instance.ThisUser.Username);
            if (string.IsNullOrEmpty(roomName))
            {
                roomName = $"{requestorUsername}.{ClientObject.Instance.ThisUser.Username}";
                IChannel channel = await ClientObject.Instance.Socket.JoinChatAsync(roomName, ChannelType.Room, true, false);
                IApiUsers users = await ClientObject.Instance.Client.GetUsersAsync(ClientObject.Instance.Session, null, new string[] { requestorUsername });
                var usersList = users.Users.ToList();
                if (usersList.Count > 0)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(CreateChannel(channel.Id, channel.RoomName, usersList[0].DisplayName));
                    AddChannelToJoined(channel.Id, channel.RoomName, usersList[0].DisplayName);
                }
            }
            else
            {
                ActivateChannel(roomName);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    private string CheckIfRoomExist(string usernameOne, string usernameTwo)
    {
        if (joinedChannels == null)
            return "";

        foreach (string channel in joinedChannels)
        {
            if (!string.IsNullOrEmpty(channel))
            {
                string[] channelDetails = channel.Split(',');
                if (channel.Contains(usernameOne) && channel.Contains(usernameTwo))
                {
                    return channelDetails[1];
                }
            }
        }
        return "";
    }    

    private List<string> GetChannels()
    {
        Debug.LogWarning($"HISTORY CHANNELS: {HISTORY_CHANNELS_STRING}");
        string historyChannelsString = PlayerPrefs.GetString(HISTORY_CHANNELS_STRING, string.Empty);
        if (historyChannelsString != string.Empty)
        {
            var channelIds = historyChannelsString.Split(';');
            return channelIds.ToList();
        }
        return new List<string>();
    }

    private IEnumerator CreateChannel(string channelId, string roomName, string toUserDisplayName)
    {
        NameplateCard nameplate = Instantiate(_nameplateCardPrefab, _activeChatsPanel);
        GameObject messagingComponents = Instantiate(_messagingComponentsPrefab, transform);
        float xPosition = Screen.width;
        messagingComponents.GetComponent<RectTransform>().DOLocalMoveX(xPosition, 0);
        nameplate.Populate(channelId, roomName, toUserDisplayName, messagingComponents);
        _chattingChannels.Add(nameplate);
        yield return null;
    }

    private void ActivateChannel(string roomName)
    {
        foreach (var chat in _chattingChannels)
        {
            if (chat.thisRoomName.Equals(roomName))
            {
                chat.ToggleComponents(true);
            }
            else
            {
                chat.ToggleComponents(false);
            }
        }
    }

    private void AddChannelToJoined(string toSaveChannelId, string roomName, string toUserDisplayName)
    {
        string toAdd;
        if (joinedChannels.Count == 0)
        {
            toAdd = $"{toSaveChannelId},{roomName},{toUserDisplayName}";
        }
        else
        {
            toAdd = $";{toSaveChannelId},{roomName},{toUserDisplayName}";
        }
        joinedChannels.Add(toAdd);
    }

    private void RemoveChannelFromJoined(string toRemoveChannelId)
    {
        int indexToRemove = -1;
        for (int i = 0; i < joinedChannels.Count; i++)
        {
            if (joinedChannels[i].Contains(toRemoveChannelId))
                indexToRemove = i;
        }
        joinedChannels.RemoveAt(indexToRemove);
    }

    private void SaveChannelsToDisk()
    {
        if (joinedChannels.Count > 0)
        {
            string stringToSave = string.Join(";", joinedChannels);
            PlayerPrefs.SetString(HISTORY_CHANNELS_STRING, stringToSave);
        }
    }

    private void SendMessageToActiveChannel(string channelId, string senderName, string message)
    {
        var messageContent = new Dictionary<string, string> { { senderName, message } };
        _ = ClientObject.Instance.Socket.WriteChatMessageAsync(channelId, messageContent.ToJson());
    }

    public void HandleNotification(IApiNotification notification)
    {
        if (notification.Subject.Contains("wants to chat"))
        {
            Dictionary<string, string> notiDetails = notification.Content.FromJson<Dictionary<string, string>>();
            AcceptDirectMessage(notiDetails.ElementAt(0).Value);
        }
    }

    public void HandleIncomingMessages(IApiChannelMessage channelMessage)
    {
        NameplateCard receivingChannel = null;
        foreach (var channel in _chattingChannels)
        {
            if (channel.thisChannelId == channelMessage.ChannelId)
            {
                receivingChannel = channel;
                break;
            }
        }
        if (receivingChannel != null)
        {
            Dictionary<string, string> messageDetails = channelMessage.Content.FromJson<Dictionary<string, string>>();
            UnityMainThreadDispatcher.Instance().Enqueue(receivingChannel.InsertMessageToContainer(
                messageDetails.ElementAt(0).Key, 
                messageDetails.ElementAt(0).Value, 
                DateTime.Parse(channelMessage.CreateTime), 
                channelMessage.Username == ClientObject.Instance.ThisUser.Username));
            UnityMainThreadDispatcher.Instance().Enqueue(receivingChannel.UpdateLastMessageToNameplate(messageDetails.ElementAt(0).Value));
        }
    }

    private async void DeleteCardAndActivateNextOne(NameplateCard cardToRemove)
    {
        await ClientObject.Instance.Socket.LeaveChatAsync(cardToRemove.thisChannelId);
        bool isActive = cardToRemove.IsActive;
        _chattingChannels.Remove(cardToRemove);
        Destroy(cardToRemove.representMessagingComponents);
        Destroy(cardToRemove.gameObject, 0.05f);

        if (isActive)
        {
            if (_chattingChannels.Count > 0)
                ActivateChannel(_chattingChannels[0].thisRoomName);
        }
        RemoveChannelFromJoined(cardToRemove.thisChannelId);
    }
}