using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class NameplateCard : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text textDisplayName;
    public GameObject representMessagingComponents;
    public Transform messageContainer;

    [Header("Prefabs")]
    [SerializeField] private MessageCard _messageCardPrefab;

    public string thisChannelId;

    public void Populate(string channelId, string userDisplayName, GameObject messagingComponents)
    {
        thisChannelId = channelId;
        textDisplayName.text = userDisplayName;
        textDisplayName.color = Color.black;
        representMessagingComponents = messagingComponents;
        messageContainer = messagingComponents.transform.GetComponentInChildren<MessagesContainer>().transform;
    }

    public void ToggleComponents(bool toggle)
    {
        textDisplayName.color = toggle ? Color.black : Color.white;
        representMessagingComponents.SetActive(toggle);
    }

    public IEnumerator InsertMessageToContainer(string displayName, string messageContent, bool isThisUser = false)
    {
        MessageCard messageCard = Instantiate(_messageCardPrefab, messageContainer);
        messageCard.Populate(displayName, messageContent, isThisUser);
        yield return null;
    }
}
