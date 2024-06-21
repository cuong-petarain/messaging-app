using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class FunctionButton : MonoBehaviour
{
    private Button button;
    [SerializeField] private Image _imageActivated;
    [SerializeField] private Image _imageInactive;
    [SerializeField] private TMP_Text _textButton;
    [SerializeField] private Color _textColorActive;
    [SerializeField] private Color _textColorInactive;

    public void ToggleActivation(bool isAcive)
    {
        _imageActivated.gameObject.SetActive(isAcive);
        _imageInactive.gameObject.SetActive(!isAcive);
        _textButton.color = isAcive ? _textColorActive : _textColorInactive;
    }
}
