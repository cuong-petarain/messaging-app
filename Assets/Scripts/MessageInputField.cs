using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MessageInputField : MonoBehaviour
{
    public string toChannelId;
    private TMP_InputField _inputField;

    private void Start()
    {
        _inputField = GetComponent<TMP_InputField>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (EventSystem.current.currentSelectedGameObject == _inputField.gameObject)
            {
                HandleSubmit();
            }
        }
    }

    private void HandleSubmit()
    {
        string cleanedText = _inputField.text.Replace("\n", "").Replace("\r", "");
        if (!string.IsNullOrEmpty(cleanedText))
        {
            PanelMessage.OnMessageSubmitted(toChannelId, ClientObject.Instance.ThisUser.DisplayName, cleanedText);
            _inputField.text = string.Empty;
        }
    }
}
