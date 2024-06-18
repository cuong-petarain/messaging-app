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
using System.Numerics;

public class PanelMessage : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform _activeChatsPanel;

    [Header("Prefabs")]
    [SerializeField] private NameplateCard _nameplateCardPrefab;
    [SerializeField] private GameObject _messagingComponentsPrefab;

    private List<NameplateCard> _chattingChannels = new List<NameplateCard>();
    private readonly string HISTORY_CHANNELS_STRING = "HistoryChannels";

    public static Action<string, string> OnSentInviteDirectMessage;
    public static Action<string, string, string> OnMessageSubmitted;
    public static Action<string> OnNameplateCardClicked;
    public static Action<NameplateCard> OnNameplateCardRemoved;

    private void OnEnable()
    {
        OnSentInviteDirectMessage += InviteDirectMessage;
        OnMessageSubmitted += SendMessageToActiveChannel;
        OnNameplateCardClicked += ActivateChannel;
        OnNameplateCardRemoved += ActivateNextCard;
    }

    private void OnDisable()
    {
        OnSentInviteDirectMessage -= InviteDirectMessage;
        OnMessageSubmitted -= SendMessageToActiveChannel;
        OnNameplateCardClicked -= ActivateChannel;
        OnNameplateCardRemoved -= ActivateNextCard;
    }

    private async void Start()
    {
        while (ClientObject.Instance.Socket == null || !ClientObject.Instance.Socket.IsConnected)
        {
            await Task.Yield();
        }
        LoadChannelsFromPlayerPrefs();
    }

    private async void LoadChannelsFromPlayerPrefs()
    {
        string[] historyChannels = GetChannels();
        if (historyChannels == null)
            return;

        foreach (string channel in historyChannels)
        {
            if (!string.IsNullOrEmpty(channel))
            {
                string[] channelDetails = channel.Split(',');
                _ = await ClientObject.Instance.Socket.JoinChatAsync(channelDetails[1], ChannelType.Room, true, false);
                StartCoroutine(CreateChannel(channelDetails[0], channelDetails[2]));
            }
        }
        ActivateChannel(_chattingChannels[0].thisChannelId);
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

    private async void InviteDirectMessage(string toUserId, string toUserName)
    {
        IChannel tmpChannel = await ClientObject.Instance.Socket.JoinChatAsync(toUserId, ChannelType.DirectMessage);  // to send notification to target user
        string roomName = ClientObject.Instance.ThisUser.Username + "." + toUserName;
        IChannel channel = await ClientObject.Instance.Socket.JoinChatAsync(roomName, ChannelType.Room, true, false);
        IApiUsers users = await ClientObject.Instance.Client.GetUsersAsync(ClientObject.Instance.Session, new string[] { toUserId });
        
        if (_chattingChannels.FirstOrDefault(u => u.thisChannelId == channel.Id) == null)
        {
            var usersList = users.Users.ToList();
            if (usersList.Count > 0)
            {
                StartCoroutine(CreateChannel(channel.Id, usersList[0].DisplayName));
                StartCoroutine(SaveChannelToDisk(channel.Id, channel.RoomName, usersList[0]));
            }
        }
        else
        {
            ActivateChannel(channel.Id);
        }

        await ClientObject.Instance.Socket.LeaveChatAsync(tmpChannel.Id);  // leave DirectMessage channel
    }

    private async void AcceptDirectMessage(string requestorUserId)
    {
        IApiUsers users = await ClientObject.Instance.Client.GetUsersAsync(ClientObject.Instance.Session, new string[] { requestorUserId });
        string roomName = users.Users.ToList()[0].Username + "." + ClientObject.Instance.ThisUser.Username;
        IChannel channel = await ClientObject.Instance.Socket.JoinChatAsync(roomName, ChannelType.Room, true, false);

        if (_chattingChannels.FirstOrDefault(u => u.thisChannelId == channel.Id) == null)
        {
            var usersList = users.Users.ToList();
            if (usersList.Count > 0)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(CreateChannel(channel.Id, usersList[0].DisplayName));
                UnityMainThreadDispatcher.Instance().Enqueue(SaveChannelToDisk(channel.Id, channel.RoomName, usersList[0]));
            }
        }
        else
        {
            ActivateChannel(requestorUserId);
        }
    }

    private IEnumerator CreateChannel(string channelId, string toUserDisplayName)
    {
        NameplateCard nameplate = Instantiate(_nameplateCardPrefab, _activeChatsPanel);
        GameObject messagingComponents = Instantiate(_messagingComponentsPrefab, transform);
        nameplate.Populate(channelId, toUserDisplayName, messagingComponents);
        _chattingChannels.Add(nameplate);
        ActivateChannel(channelId);
        yield return null;
    }

    private void ActivateChannel(string channelId)
    {
        foreach (var chat in _chattingChannels)
        {
            if (chat.thisChannelId.Equals(channelId))
            {
                chat.ToggleComponents(true);
            }
            else
            {
                chat.ToggleComponents(false);
            }
        }
    }

    private IEnumerator SaveChannelToDisk(string toSaveChannelId, string roomName, IApiUser user)
    {
        string historyChannelsString = PlayerPrefs.GetString(HISTORY_CHANNELS_STRING, string.Empty);
        if (historyChannelsString != string.Empty)
        {
            var channelIds = historyChannelsString.Split(';');
            if (channelIds.FirstOrDefault(c => c == toSaveChannelId) == null)
            {
                historyChannelsString += $";{toSaveChannelId},{roomName},{user.DisplayName}";
                PlayerPrefs.SetString(HISTORY_CHANNELS_STRING, historyChannelsString);
            }
        }
        else
        {
            historyChannelsString += $"{toSaveChannelId},{roomName},{user.DisplayName}";
            PlayerPrefs.SetString(HISTORY_CHANNELS_STRING, historyChannelsString);
        }
        yield return null;
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
            AcceptDirectMessage(notification.SenderId);
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
        }
    }

    private async void ActivateNextCard(NameplateCard cardToRemove)
    {
        await ClientObject.Instance.Socket.LeaveChatAsync(cardToRemove.thisChannelId);
        bool isActive = cardToRemove.IsActive;
        _chattingChannels.Remove(cardToRemove);
        Destroy(cardToRemove.representMessagingComponents);
        Destroy(cardToRemove.gameObject, 0.05f);

        if (isActive)
        {
            if (_chattingChannels.Count > 0)
                ActivateChannel(_chattingChannels[0].thisChannelId);
        }
        RemoveChannelFromDisk(cardToRemove.thisChannelId);
    }

    private void RemoveChannelFromDisk(string toRemoveChannelId)
    {
        string historyChannelsString = PlayerPrefs.GetString(HISTORY_CHANNELS_STRING, string.Empty);
        if (historyChannelsString != string.Empty)
        {
            var channelIds = historyChannelsString.Split(';');
            int indexToRemove = 0;
            for (int i = 0; i < channelIds.Length; i++)
            {
                if (channelIds[i].Contains(toRemoveChannelId))
                {
                    indexToRemove = i;
                    break;
                }
            }
            string[] newArray = RemoveElement(channelIds, channelIds[indexToRemove]);
            string newHistoryChanningString = ConvertArrayToString(newArray, ';');
            PlayerPrefs.SetString(HISTORY_CHANNELS_STRING, newHistoryChanningString);
        }
    }

    string[] RemoveElement(string[] originalArray, string elementToRemove)
    {
        int index = System.Array.IndexOf(originalArray, elementToRemove);

        if (index < 0)
        {
            // Element not found
            return originalArray;
        }

        string[] newArray = new string[originalArray.Length - 1];

        for (int i = 0, j = 0; i < originalArray.Length; i++)
        {
            if (i == index)
            {
                continue;
            }
            newArray[j++] = originalArray[i];
        }

        return newArray;
    }

    string ConvertArrayToString(string[] array, char separator)
    {
        return string.Join(separator.ToString(), array);
    }
}