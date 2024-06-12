using Nakama;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Nakama.TinyJson;
using PimDeWitte.UnityMainThreadDispatcher;

public class PanelMessage : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform _activeChatsPanel;

    [Header("Prefabs")]
    [SerializeField] private NameplateCard _nameplateCardPrefab;
    [SerializeField] private GameObject _messagingComponentsPrefab;

    private List<NameplateCard> _chattingChannels = new List<NameplateCard>();
    private readonly string HISTORY_CHANNELS_STRING = "HistoryChannels";

    public static Action<string> OnSentInviteDirectMessage;
    public static Action<string, string, string> OnMessageSubmitted;

    private void OnEnable()
    {
        OnSentInviteDirectMessage += InviteDirectMessage;
        OnMessageSubmitted += SendMessageToActiveChannel;
    }

    private void OnDisable()
    {
        OnSentInviteDirectMessage -= InviteDirectMessage;
        OnMessageSubmitted -= SendMessageToActiveChannel;
    }

    private async void Start()
    {
        while (ClientObject.Instance.Socket == null || !ClientObject.Instance.Socket.IsConnected)
        {
            await Task.Yield();
        }
        StartCoroutine(LoadChannelsFromHistory());
    }

    private async void InviteDirectMessage(string toUserId)
    {
        if (_chattingChannels.FirstOrDefault(u => u.thisChannelId == toUserId) == null)
        {
            ActivateChat(toUserId);
            IChannel tmpChannel = await ClientObject.Instance.Socket.JoinChatAsync(toUserId, ChannelType.DirectMessage);  // to send notification to target user
            IChannel channel = await ClientObject.Instance.Socket.JoinChatAsync(ClientObject.Instance.ThisUser.Id, ChannelType.Room, true, false);
            IApiUsers users = await ClientObject.Instance.Client.GetUsersAsync(ClientObject.Instance.Session, new string[] { toUserId });
            var usersList = users.Users.ToList();
            if (usersList.Count > 0)
            {
                StartCoroutine(CreateNewChat(channel.Id, usersList[0].DisplayName));
                StartCoroutine(SaveChannel(channel.Id, usersList[0]));
            }
            await ClientObject.Instance.Socket.LeaveChatAsync(tmpChannel.Id);  // leave DirectMessage channel
        }
        else
        {
            ActivateChat(toUserId);
        }
    }

    private IEnumerator CreateNewChat(string channelId, string toUserDisplayName)
    {
        NameplateCard nameplate = Instantiate(_nameplateCardPrefab, _activeChatsPanel);
        GameObject messagingComponents = Instantiate(_messagingComponentsPrefab, transform);
        nameplate.Populate(channelId, toUserDisplayName, messagingComponents);
        _chattingChannels.Add(nameplate);

        yield return null;
    }

    private void ActivateChat(string userId)
    {
        foreach (var chat in _chattingChannels)
        {
            if (chat.thisChannelId.Contains(userId) && chat.thisChannelId.Contains(ClientObject.Instance.ThisUser.Id))
            {
                chat.ToggleComponents(true);
            }
            else
            {
                chat.ToggleComponents(false);
            }
        }
    }

    public void HandleNotification(IApiNotification notification)
    {
        if (notification.Subject.Contains("wants to chat"))
        {
            AcceptDirectMessage(notification.SenderId);
        }
    }

    private async void AcceptDirectMessage(string requestorUserId)
    {
        if (_chattingChannels.FirstOrDefault(u => u.thisChannelId == requestorUserId) == null)
        {
            IChannel channel = await ClientObject.Instance.Socket.JoinChatAsync(requestorUserId, ChannelType.Room, true, false);
            
            IApiUsers users = await ClientObject.Instance.Client.GetUsersAsync(ClientObject.Instance.Session, new string[] { requestorUserId });
            var usersList = users.Users.ToList();
            if (usersList.Count > 0)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(CreateNewChat(channel.Id, usersList[0].DisplayName));
                UnityMainThreadDispatcher.Instance().Enqueue(SaveChannel(channel.Id, usersList[0]));
            }
        }
        else
        {
            ActivateChat(requestorUserId);
        }
    }

    private void SendMessageToActiveChannel(string channelId, string senderName, string message)
    {
        var messageContent = new Dictionary<string, string> { { senderName, message } };
        _ = ClientObject.Instance.Socket.WriteChatMessageAsync(channelId, messageContent.ToJson());
    }

    public void HandleIncomingMessages(IApiChannelMessage channelMessage)
    {
        Debug.Log("GOT MESSAGE FROM ROOM: " + channelMessage.RoomName);
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
            UnityMainThreadDispatcher.Instance().Enqueue(receivingChannel.InsertMessageToContainer(messageDetails.ElementAt(0).Key, messageDetails.ElementAt(0).Value, channelMessage.Username == ClientObject.Instance.ThisUser.Username));
        }
    }

    private IEnumerator LoadChannelsFromHistory()
    {
        string[] historyChannels = GetChannels();
        if (historyChannels == null)
            yield break;

        foreach (string channel in historyChannels)
        {
            if (!string.IsNullOrEmpty(channel))
            {
                string[] channelDetails = channel.Split(',');
                yield return StartCoroutine(CreateNewChat(channelDetails[0], channelDetails[2]));
                GetMessageHistoryOfChannel(channelDetails[0]);
            }
        }
    }

    private async void GetMessageHistoryOfChannel(string channelId)
    {
        NameplateCard receivingChannel = null;
        foreach (var channel in _chattingChannels)
        {
            if (channel.thisChannelId == channelId)
            {
                receivingChannel = channel;
                break;
            }
        }

        IApiChannelMessageList result = await ClientObject.Instance.Client.ListChannelMessagesAsync(ClientObject.Instance.Session, channelId, 10, true);
        List<IApiChannelMessage> listMessages = result.Messages.ToList();
        if (listMessages.Count > 0)
        {
            foreach(var channelMessage in listMessages)
            {
                Dictionary<string, string> messageDetails = channelMessage.Content.FromJson<Dictionary<string, string>>();
                UnityMainThreadDispatcher.Instance().Enqueue(receivingChannel.InsertMessageToContainer(messageDetails.ElementAt(0).Key, messageDetails.ElementAt(0).Value, channelMessage.Username == ClientObject.Instance.ThisUser.Username));
            }
        }
    }

    private IEnumerator SaveChannel(string toSaveChannelId, IApiUser user)
    {
        string historyChannelsString = PlayerPrefs.GetString(HISTORY_CHANNELS_STRING, string.Empty);
        if (historyChannelsString != string.Empty)
        {
            var channelIds = historyChannelsString.Split(';');
            if (channelIds.FirstOrDefault(c => c == toSaveChannelId) == null)
            {
                historyChannelsString += $";{toSaveChannelId},{user.Id},{user.DisplayName}";
                PlayerPrefs.SetString(HISTORY_CHANNELS_STRING, historyChannelsString);
            }
        }
        else
        {
            historyChannelsString += $"{toSaveChannelId},{user.Id},{user.DisplayName}";
            PlayerPrefs.SetString(HISTORY_CHANNELS_STRING, historyChannelsString);
        }
        yield return null;
    }

    private string[] GetChannels()
    {
        string historyChannelsString = PlayerPrefs.GetString(HISTORY_CHANNELS_STRING, string.Empty);
        if (historyChannelsString != string.Empty)
        {
            var channelIds = historyChannelsString.Split(';');
            return channelIds;
        }
        return null;
    }
}