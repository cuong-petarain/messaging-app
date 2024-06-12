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
    private string _activeChannelId;

    public static Action<string> OnSentInviteDirectMessage;
    public static Action<string, string> OnMessageSubmitted;

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

    private async void InviteDirectMessage(string toUserId)
    {
        if (_chattingChannels.FirstOrDefault(u => u.thisChannelId == toUserId) == null)
        {
            ActivateChat(toUserId);
            bool persistence = true;
            bool hidden = false;
            var channel = await ClientObject.Instance.Socket.JoinChatAsync(toUserId, Nakama.ChannelType.DirectMessage, persistence, hidden);
            _activeChannelId = channel.Id;

            var users = await ClientObject.Instance.Client.GetUsersAsync(ClientObject.Instance.Session, new string[] { toUserId });
            var usersList = users.Users.ToList();
            if (usersList.Count > 0)
            {
                StartCoroutine(CreateNewChat(usersList[0].DisplayName));
            }
        }
        else
        {
            ActivateChat(toUserId);
        }
    }

    private IEnumerator CreateNewChat(string toUserDisplayName)
    {
        NameplateCard nameplate = Instantiate(_nameplateCardPrefab, _activeChatsPanel);
        GameObject messagingComponents = Instantiate(_messagingComponentsPrefab, transform);
        nameplate.Populate(_activeChannelId, toUserDisplayName, messagingComponents);
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
            bool persistence = true;
            bool hidden = false;
            var channel = await ClientObject.Instance.Socket.JoinChatAsync(requestorUserId, Nakama.ChannelType.DirectMessage, persistence, hidden);
            _activeChannelId = channel.Id;

            var users = await ClientObject.Instance.Client.GetUsersAsync(ClientObject.Instance.Session, new string[] { requestorUserId });
            var usersList = users.Users.ToList();
            if (usersList.Count > 0)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(CreateNewChat(usersList[0].DisplayName));
            }
        }
        else
        {
            ActivateChat(requestorUserId);
        }
    }

    private void SendMessageToActiveChannel(string senderName, string message)
    {
        var messageContent = new Dictionary<string, string> { { senderName, message } };
        _ = ClientObject.Instance.Socket.WriteChatMessageAsync(_activeChannelId, messageContent.ToJson());
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
            UnityMainThreadDispatcher.Instance().Enqueue(receivingChannel.InsertMessageToContainer(messageDetails.ElementAt(0).Key, messageDetails.ElementAt(0).Value, channelMessage.Username == ClientObject.Instance.ThisUser.Username));
        }
    }
}