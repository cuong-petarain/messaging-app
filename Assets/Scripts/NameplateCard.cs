using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class NameplateCard : MonoBehaviour
{
    public TMP_Text textDisplayName;
    public GameObject representMessagingComponents;

    public string thisChannelId;

    public void Populate(string userDisplayName, GameObject messagingComponents)
    {
        textDisplayName.text = userDisplayName;
        textDisplayName.color = Color.black;
        representMessagingComponents = messagingComponents;
    }

    public void ToggleComponents(bool toggle)
    {
        textDisplayName.color = toggle ? Color.black : Color.white;
        representMessagingComponents.SetActive(toggle);
    }
}
