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

    public void Populate(string userId, string displayName, bool isOnline)
    {
        _userId = userId;
        _textName.text = displayName;
        _toggleStatus.isOn = isOnline;
    }

    public void OnCardClicked()
    {
        if (_userId != ClientObject.Instance.thisUserId)
            PanelMessage.OnSentInviteDirectMessage?.Invoke(_userId);
    }
}
