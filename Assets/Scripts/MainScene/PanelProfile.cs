using Nakama;
using System;
using TMPro;
using UnityEngine;

public class PanelProfile : MonoBehaviour
{
    [SerializeField] private TMP_Text _textDisplayName;
    [SerializeField] private TMP_Text _textEmail;
    [SerializeField] private TMP_InputField _inputFieldEditName;

    private bool _isEditing;
    private IApiAccount _account;

    public static Action<string> OnUserInfoChanged;

    public void Populate(IApiAccount accountInfo)
    {
        _account = accountInfo;
        _textDisplayName.text = accountInfo.User.DisplayName;
        _inputFieldEditName.text = accountInfo.User.DisplayName;
        _textEmail.text = accountInfo.Email;
    }

    public async void OnUpdateInfoClicked()
    {
        _isEditing = !_isEditing;

        _inputFieldEditName.gameObject.SetActive(_isEditing);

        if (_isEditing)
        {
            _textDisplayName.gameObject.SetActive(false);
            _inputFieldEditName.gameObject.SetActive(true);
        }
        else
        {
            _textDisplayName.text = _inputFieldEditName.text;
            _textDisplayName.gameObject.SetActive(true);
            _inputFieldEditName.gameObject.SetActive(false);

            await ClientObject.Instance.Client.UpdateAccountAsync(ClientObject.Instance.Session, _account.User.Username, _inputFieldEditName.text);
            OnUserInfoChanged?.Invoke(_inputFieldEditName.text);
        }
    }
}
