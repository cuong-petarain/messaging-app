using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UserStatusCard : MonoBehaviour
{
    [SerializeField] private TMP_Text _textName;
    [SerializeField] private TMP_Text _textOnlineStatus;
    [SerializeField] private Image _imageAvatar;

    private string _userId;
    private string _userName;

    public void Populate(string userId, string userName, string displayName, bool isOnline)
    {
        _userId = userId;
        _userName = userName;
        _textName.text = displayName;
        _textOnlineStatus.text = isOnline ? "online" : "offline";
        _textOnlineStatus.color = isOnline ? Color.green : Color.gray;
    }

    public void OnCardClicked()
    {
        if (_userId != ClientObject.Instance.ThisUser.Id)
            PanelMessage.OnSentInviteDirectMessage?.Invoke(_userId, _userName);
    }
}
