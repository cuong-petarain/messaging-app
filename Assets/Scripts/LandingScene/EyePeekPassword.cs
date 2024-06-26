using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EyePeekPassword : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Sprite _spriteEyeOff;
    [SerializeField] private Sprite _spriteEyeOn;

    private Image _image;
    private TMP_InputField _inputField;

    private void Start()
    {
        _image = GetComponent<Image>();
        _inputField = GetComponentInParent<TMP_InputField>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_inputField == null)
            return;
        _image.sprite = _spriteEyeOff;
        _inputField.contentType = TMP_InputField.ContentType.Standard;
        _inputField.textComponent.SetAllDirty();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_inputField == null)
            return;
        _image.sprite = _spriteEyeOn;
        _inputField.contentType = TMP_InputField.ContentType.Password;
        _inputField.textComponent.SetAllDirty();
    }
}
