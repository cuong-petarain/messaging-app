using Nakama;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Nakama.TinyJson;
using System;

public class NameplateCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public string thisChannelId;
    public string thisRoomName;
    public bool IsActive;

    [Header("UI")]
    public TMP_Text textDisplayName;
    public TMP_Text textLastMessage;
    public Image backgroundImage;
    public Image avatarImage;
    [SerializeField] private Color _pointerEnterColor;
    [SerializeField] private Color _pointerClickColor;

    [Header("References")]
    public GameObject representMessagingComponents;
    public Transform messageContainer;

    [Header("Prefabs")]
    [SerializeField] private MessageCard _messageCardPrefab;
    [SerializeField] private Sprite[] _defaultSprites = new Sprite[7];

    private string _nextCursor;
    private ScrollRect _messagesScrollRect;
    private readonly float FETCH_THRESHOLD = 0.9f;
    private int _messageCount = 40;
    private float _cooldownHistory = 0.5f;
    private float _lastFetchHistoryTime = 0;
    private List<MessageCard> _messageCards = new List<MessageCard>();
    private List<DateTime> _dateGroups = new List<DateTime>();
    private List<GameObject> dateGroupObjects = new List<GameObject>();

    public void Populate(string channelId, string roomName, string userDisplayName, GameObject messagingComponents)
    {
        thisChannelId = channelId;
        thisRoomName = roomName;
        textDisplayName.text = userDisplayName;
        textDisplayName.color = Color.black;
        avatarImage.sprite = _defaultSprites[UnityEngine.Random.Range(0, _defaultSprites.Length)];

        representMessagingComponents = messagingComponents;
        messagingComponents.transform.Find("Text_NameChat").GetComponent<TMP_Text>().text = userDisplayName;
        messagingComponents.transform.Find("Image_Avatar").GetComponent<Image>().sprite = avatarImage.sprite;
        _messagesScrollRect = messagingComponents.GetComponentInChildren<ScrollRect>();
        _messagesScrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        messageContainer = messagingComponents.GetComponentInChildren<MessagesContainer>().transform;
        var messageInputField = messagingComponents.GetComponentInChildren<MessageInputField>();
        messageInputField.toChannelId = channelId;
        var buttonBack = messagingComponents.GetComponentInChildren<Button>();
        buttonBack.onClick.AddListener(() => BackToNames());
        ToggleComponents(false);
        FetchMessageHistoryOfChannel();
    }

    private void BackToNames()
    {
        ToggleComponents(false);
    }

    public void ToggleComponents(bool toggle)
    {
        IsActive = toggle;
        textDisplayName.color = toggle ? Color.black : Color.white;
        representMessagingComponents.SetActive(toggle);
        backgroundImage.color = toggle ? _pointerClickColor : Color.white;
        backgroundImage.enabled = toggle;
    }

    public void UpdateLastMessageToNameplate(string messageContent)
    {
        if (messageContent.Length >= 50)
            textLastMessage.text = messageContent[..50];
        else
            textLastMessage.text = messageContent;
    }

    private async void FetchMessageHistoryOfChannel()
    {
        IApiChannelMessageList result = await ClientObject.Instance.Client.ListChannelMessagesAsync(ClientObject.Instance.Session, thisChannelId, _messageCount, false);
        _nextCursor = result.NextCursor;
        List<IApiChannelMessage> listMessages = result.Messages.ToList();
        if (listMessages.Count > 0)
        {
            for (int i = 0; i < listMessages.Count; i++)
            {
                Dictionary<string, string> messageDetails = listMessages[i].Content.FromJson<Dictionary<string, string>>();
                UnityMainThreadDispatcher.Instance().Enqueue(InsertMessageToContainer(
                    messageDetails.ElementAt(0).Key, 
                    messageDetails.ElementAt(0).Value, 
                    DateTime.Parse(listMessages[i].CreateTime),
                    listMessages[i].Username == ClientObject.Instance.ThisUser.Username, true));

                if (i == 0)
                {
                    UpdateLastMessageToNameplate(messageDetails.ElementAt(0).Value);
                }
            }
            UnityMainThreadDispatcher.Instance().Enqueue(ProcessDateTimePosted());
        }
    }

    public IEnumerator InsertMessageToContainer(string displayName, string messageContent, DateTime createTime, bool isThisUser = false, bool isPastMessage = false)
    {
        if (!_dateGroups.Contains(createTime.Date))
        {
            MessageCard dateCard = Instantiate(_messageCardPrefab, messageContainer);
            dateCard.PopulateDateGroup(createTime);
            dateGroupObjects.Add(dateCard.gameObject);
            _dateGroups.Add(createTime.Date);
        }

        MessageCard newMessage = Instantiate(_messageCardPrefab, messageContainer);
        if (isPastMessage)
        {
            newMessage.transform.SetSiblingIndex(0);
        }
        newMessage.Populate(displayName, messageContent, createTime, isThisUser);
        _messageCards.Add(newMessage);
        yield return null;
    }

    private IEnumerator ProcessDateTimePosted()
    {
        foreach (var d in dateGroupObjects)
            Destroy(d);
        dateGroupObjects.Clear();
        _dateGroups.Clear();

        var groupedMessages = _messageCards.GroupBy(m => m.createDateTime.Date).ToList();
        foreach (var group in groupedMessages)
        {
            if (_dateGroups.Contains(group.Key))
                yield break;
            MessageCard earliestMessage = null;
            DateTime earliest = new DateTime(group.Key.Year, group.Key.Month, group.Key.Day, 23, 59, 59, 999);
            foreach (var message in group)
            {
                if (message.createDateTime <= earliest)
                {
                    earliestMessage = message;
                    earliest = message.createDateTime;
                }
            }
            MessageCard dateCard = Instantiate(_messageCardPrefab, messageContainer);
            dateCard.transform.SetSiblingIndex(earliestMessage.transform.GetSiblingIndex());
            dateCard.PopulateDateGroup(group.Key);
            dateGroupObjects.Add(dateCard.gameObject);
            _dateGroups.Add(group.Key);
        }
        yield return null;
    }

    private async void OnScrollValueChanged(Vector2 scrollValue)
    {
        if (string.IsNullOrEmpty(_nextCursor))
            return;

        if (_messagesScrollRect.verticalNormalizedPosition >= FETCH_THRESHOLD)
        {
            if (Time.timeSinceLevelLoad < _lastFetchHistoryTime + _cooldownHistory)
                return;
            _lastFetchHistoryTime = Time.timeSinceLevelLoad;

            IApiChannelMessageList result = await ClientObject.Instance.Client.ListChannelMessagesAsync(ClientObject.Instance.Session, thisChannelId, _messageCount, false, _nextCursor);
            _nextCursor = result.NextCursor;
            List<IApiChannelMessage> listMessages = result.Messages.ToList();
            if (listMessages.Count > 0)
            {
                foreach (var channelMessage in listMessages)
                {
                    Dictionary<string, string> messageDetails = channelMessage.Content.FromJson<Dictionary<string, string>>();
                    UnityMainThreadDispatcher.Instance().Enqueue(InsertMessageToContainer(
                        messageDetails.ElementAt(0).Key, 
                        messageDetails.ElementAt(0).Value, 
                        DateTime.Parse(channelMessage.CreateTime), 
                        channelMessage.Username == ClientObject.Instance.ThisUser.Username, true));
                }
                UnityMainThreadDispatcher.Instance().Enqueue(ProcessDateTimePosted());
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsActive)
            return;
        backgroundImage.color = _pointerEnterColor;
        backgroundImage.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsActive)
            return;
        backgroundImage.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            PanelMessage.OnNameplateCardClicked?.Invoke(thisRoomName);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            PanelMessage.OnNameplateCardRemoved?.Invoke(this);
        }
    }

    private void OnDisable()
    {
        _messagesScrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
    }
}
