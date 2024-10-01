using Nakama;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PanelLogin : MonoBehaviour
{
    [SerializeField] private Image _imageEmailInput;
    [SerializeField] private GameObject _errorContainer;
    [SerializeField] private TMP_Text _textErrorMessage;
    [SerializeField] private TMP_InputField _inputEmail;
    [SerializeField] private TMP_InputField _inputPassword;
    [SerializeField] private Button _buttonLogin;
    [SerializeField] private Button _buttonGoToRegister;
    [SerializeField] private GameObject _imageLoading;
    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _erroredColor;

    private readonly string INVALID_EMAIL = "Invalid email address.";
    private readonly string INCORRECT_EMAIL_PASSWORD = "Email or password is incorrect.";
    private readonly string MAIN_SCENE_NAME = "SampleScene";

    public async void OnLoginButtonClicked()
    {
        bool isReadyToLogin = CheckConditionsToLogin();
        if (!isReadyToLogin)
        {
            return;
        }

        try
        {
            bool connectSuccessfully = await ClientObject.Instance.AuthenticateAsync(_inputEmail.text, _inputPassword.text, false);
            if (connectSuccessfully)
            {
                StartCoroutine(ChangeScene(MAIN_SCENE_NAME));
            }
        }
        catch
        {
            SetErrorMessage(INCORRECT_EMAIL_PASSWORD);
        }
    }

    private IEnumerator ChangeScene(string sceneName)
    {
        _imageLoading.SetActive(true);
        yield return new WaitForSeconds(1);
        SceneManager.LoadSceneAsync(sceneName);
    }

    private bool CheckConditionsToLogin()
    {
        ToggleButtons(false);
        ResetVisuals();

        if (!IsValidEmail(_inputEmail.text))
        {
            SetErrorMessage(INVALID_EMAIL);
            _imageEmailInput.color = _erroredColor;
            return false;
        }

        return true;
    }

    private void ResetVisuals()
    {
        _imageEmailInput.color = _normalColor;
        SetErrorMessage(string.Empty);
    }

    private void ToggleButtons(bool toggle)
    {
        _buttonLogin.interactable = toggle;
        _buttonGoToRegister.interactable = toggle;
    }

    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    private void SetErrorMessage(string message)
    {
        ToggleButtons(true);
        _errorContainer.SetActive(!string.IsNullOrEmpty(message));
        _textErrorMessage.text = message;
    }
}
