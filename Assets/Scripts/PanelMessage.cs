using Nakama;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class PanelMessage : MonoBehaviour
{
    private static PanelMessage instance;
    public static PanelMessage Instance => instance;

    [Header("UI")]
    [SerializeField] private Transform _activeChatsPanel;

    [Header("Prefabs")]
    [SerializeField] private NameplateCard _nameplateCardPrefab;
    [SerializeField] private GameObject _messagingComponentsPrefab;

    private List<NameplateCard> _chattingUsers = new List<NameplateCard>();

    public static Action<string> OnSentInviteDirectMessage;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void OnEnable()
    {
        OnSentInviteDirectMessage += InviteDirectMessage;
    }

    private void OnDisable()
    {
        OnSentInviteDirectMessage -= InviteDirectMessage;
    }

    private async void InviteDirectMessage(string toUserId)
    {
        if (_chattingUsers.FirstOrDefault(u => u.thisUserId == toUserId) == null)
        {
            ActivateChat(toUserId);
            bool persistence = true;
            bool hidden = false;
            var channel = await ClientObject.Instance.Socket.JoinChatAsync(toUserId, Nakama.ChannelType.DirectMessage, persistence, hidden);
            CreateNewChat(toUserId);
        }
        else
        {
            ActivateChat(toUserId);
        }
    }

    private void CreateNewChat(string userId)
    {
        NameplateCard nameplate = Instantiate(_nameplateCardPrefab, _activeChatsPanel);
        GameObject messagingComponents = Instantiate(_messagingComponentsPrefab, transform);
        nameplate.Populate(userId, messagingComponents);

        _chattingUsers.Add(nameplate);
    }

    private void ActivateChat(string userId)
    {
        foreach (var chat in _chattingUsers)
        {
            if (chat.thisUserId == userId)
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
        if (_chattingUsers.FirstOrDefault(u => u.thisUserId == requestorUserId) == null)
        {
            bool persistence = true;
            bool hidden = false;
            var channel = await ClientObject.Instance.Socket.JoinChatAsync(requestorUserId, Nakama.ChannelType.DirectMessage, persistence, hidden);
            CreateNewChat(requestorUserId);
        }
        else
        {
            ActivateChat(requestorUserId);
        }
    }
}
