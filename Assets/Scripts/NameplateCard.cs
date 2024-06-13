using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NameplateCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI")]
    public TMP_Text textDisplayName;
    public Image backgroundImage;
    [SerializeField] private Color _pointerEnterColor;
    [SerializeField] private Color _pointerClickColor;

    [Header("References")]
    public GameObject representMessagingComponents;
    public Transform messageContainer;

    [Header("Prefabs")]
    [SerializeField] private MessageCard _messageCardPrefab;

    public string thisChannelId;
    public bool IsActive;

    public void Populate(string channelId, string userDisplayName, GameObject messagingComponents)
    {
        thisChannelId = channelId;
        textDisplayName.text = userDisplayName;
        textDisplayName.color = Color.black;
        representMessagingComponents = messagingComponents;
        messageContainer = messagingComponents.transform.GetComponentInChildren<MessagesContainer>().transform;
        var messageInputField = messagingComponents.transform.GetComponentInChildren<MessageInputField>();
        messageInputField.toChannelId = channelId;
    }

    public void ToggleComponents(bool toggle)
    {
        IsActive = toggle;
        textDisplayName.color = toggle ? Color.black : Color.white;
        representMessagingComponents.SetActive(toggle);
        backgroundImage.color = toggle ? _pointerClickColor : Color.white;
        backgroundImage.enabled = toggle;
    }

    public IEnumerator InsertMessageToContainer(string displayName, string messageContent, bool isThisUser = false)
    {
        MessageCard messageCard = Instantiate(_messageCardPrefab, messageContainer);
        messageCard.Populate(displayName, messageContent, isThisUser);
        yield return null;
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
            PanelMessage.OnNameplateCardClicked?.Invoke(thisChannelId);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            PanelMessage.OnNameplateCardRemoved?.Invoke(this);
        }
    }
}
