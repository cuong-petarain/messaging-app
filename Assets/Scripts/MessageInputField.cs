using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MessageInputField : MonoBehaviour
{
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
        PanelMessage.OnMessageSubmitted(_inputField.text);
        _inputField.text = string.Empty;
    }
}
