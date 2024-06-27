using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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

    private int _characterLimit = 60;

    public void Populate(string displayName, string messageContent, DateTime createTime, bool isThisUser = false)
    {
        //_textDisplayName.text = displayName + ":";  // commented because conversations between 2 don't need names
        _textMessageContent.text = InsertEveryXChars(messageContent, "<br>", _characterLimit);
        createDateTime = createTime;
        _textSentTime.text = $"{createTime.ToShortTimeString()}";
        if (isThisUser)
        {
            _backgroundImage.color = _thisUserBackgroundColor;
            _horizontalLayoutGroup.childAlignment = TextAnchor.UpperRight;
            _textSentTime.transform.SetSiblingIndex(0);
        }
        else
        {
            _backgroundImage.color = _otherUserBackgroundColor;
            _horizontalLayoutGroup.childAlignment = TextAnchor.UpperLeft;
            _textSentTime.transform.SetSiblingIndex(1);
        }
    }

    public void PopulateDateGroup(DateTime createTime)
    {
        _textMessageContent.color = Color.white;
        _textMessageContent.text = createTime.ToString("MMMM d, yyyy");
        _textMessageContent.fontSize += 2;
        _textSentTime.gameObject.SetActive(false);
    }

    private string InsertEveryXChars(string original, string insertion, int interval)
    {
        if (interval <= 0)
        {
            throw new ArgumentException("Interval must be greater than zero.");
        }

        if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(insertion))
        {
            return original;
        }

        StringBuilder modifiedString = new StringBuilder();

        for (int i = 0; i < original.Length; i++)
        {
            modifiedString.Append(original[i]);
            if ((i + 1) % interval == 0 && i != original.Length - 1)
            {
                modifiedString.Append(insertion);
            }
        }

        return modifiedString.ToString();
    }
}
