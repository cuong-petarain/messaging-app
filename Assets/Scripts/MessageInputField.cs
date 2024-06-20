using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MessageInputField : MonoBehaviour
{
    public string toChannelId;
    [SerializeField] private Button _submitButton;
    private TMP_InputField _inputField;

    private int lineCharacterLimit = 52;
    private int heightBase = 60;
    private int increaseHeightStep = 40;
    private int lineAdded = 0;

    private void Start()
    {
        _inputField = GetComponent<TMP_InputField>();
        _submitButton.onClick.AddListener(HandleSubmit);
        _inputField.onValueChanged.AddListener(delegate { CountLines(); });
    }

    private void OnDestroy()
    {
        _submitButton.onClick.RemoveListener(HandleSubmit);
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

    private void CountLines()
    {
        string text = _inputField.text;  
        int currentLine = Mathf.FloorToInt(text.Length / lineCharacterLimit);
        if (currentLine != lineAdded)
        {
            Vector2 sizeDelta = _inputField.GetComponent<RectTransform>().sizeDelta;
            lineAdded = currentLine;
            sizeDelta.y = heightBase + (lineAdded * increaseHeightStep);
            _inputField.GetComponent<RectTransform>().sizeDelta = sizeDelta;
        }
    }
}
