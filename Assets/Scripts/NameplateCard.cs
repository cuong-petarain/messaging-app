using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class NameplateCard : MonoBehaviour
{
    public TMP_Text textDisplayName;
    public GameObject representMessagingComponents;

    public string thisUserId;

    public async void Populate(string userId, GameObject messagingComponents)
    {
        thisUserId = userId;
        var users = await ClientObject.Instance.Client.GetUsersAsync(ClientObject.Instance.Session, new string[] { userId });
        var usersList = users.Users.ToList();
        if (usersList.Count > 0)
        {
            textDisplayName.text = usersList[0].DisplayName;
            textDisplayName.color = Color.black;
            representMessagingComponents = messagingComponents;
        }
    }

    public void ToggleComponents(bool toggle)
    {
        textDisplayName.color = toggle ? Color.black : Color.white;
        representMessagingComponents.SetActive(toggle);
    }
}
