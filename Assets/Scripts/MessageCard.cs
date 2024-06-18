using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageCard : MonoBehaviour
{
    [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroup;
    [SerializeField] private TMP_Text _textDisplayName;
    [SerializeField] private TMP_Text _textSentTime;
    [SerializeField] private TMP_Text _textMessageContent;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Color _thisUserBackgroundColor;
    [SerializeField] private Color _otherUserBackgroundColor;

    public DateTime createDateTime;

    public void Populate(string displayName, string messageContent, DateTime createTime, bool isThisUser = false)
    {
        _textDisplayName.text = displayName + ":";
        _textMessageContent.text = messageContent;
        createDateTime = createTime;
        _textSentTime.text = $"[{createTime.ToShortTimeString()}]";
        if (isThisUser)
        {
            _backgroundImage.color = _thisUserBackgroundColor;
            _horizontalLayoutGroup.childAlignment = TextAnchor.UpperRight;
        }
        else
        {
            _backgroundImage.color = _otherUserBackgroundColor;
            _horizontalLayoutGroup.childAlignment = TextAnchor.UpperLeft;
        }
    }

    public void PopulateDateGroup(string dateString)
    {
        _textSentTime.text = dateString;
        _textSentTime.fontSize += 2;
        _textSentTime.color = Color.black;
    }
}
