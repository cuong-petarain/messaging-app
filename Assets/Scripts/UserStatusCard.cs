using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UserStatusCard : MonoBehaviour
{
    [SerializeField] private TMP_Text _textName;
    [SerializeField] private Toggle _toggleStatus;

    private string _userId;
    private string _userName;

    public void Populate(string userId, string userName, string displayName, bool isOnline)
    {
        _userId = userId;
        _userName = userName;
        _textName.text = displayName;
        _toggleStatus.isOn = isOnline;
    }

    public void OnCardClicked()
    {
        if (_userId != ClientObject.Instance.ThisUser.Id)
            PanelMessage.OnSentInviteDirectMessage?.Invoke(_userId, _userName);
    }
}
