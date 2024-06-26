using Nakama;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PanelRegister : MonoBehaviour
{
    [SerializeField] private Image _imageEmailInput;
    [SerializeField] private Image _imagePasswordInput;
    [SerializeField] private Image _imageConPasswordInput;
    [SerializeField] private Toggle _toggleTos;
    [SerializeField] private TMP_Text _textTos;
    [SerializeField] private GameObject _errorContainer;
    [SerializeField] private TMP_Text _textErrorMessage;
    [SerializeField] private TMP_InputField _inputEmail;
    [SerializeField] private TMP_InputField _inputPassword;
    [SerializeField] private TMP_InputField _inputConPassword;
    [SerializeField] private GameObject _imageLoading;
    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _erroredColor;

    private readonly string NO_CHECK_TERMS_CONDITIONS = "Please read and agree to the terms and conditions.";
    private readonly string MISMATCH_PASSWORD_CONFIRMATION = "The password confirmation does not match.";
    private readonly string INVALID_EMAIL = "Invalid email address.";
    private readonly string EXISTED_EMAIL = "This email address already existed.";
    private readonly string MAIN_SCENE_NAME = "SampleScene";

    private void OnEnable()
    {
        ResetVisuals();
    }

    public async void OnRegisterButtonClicked()
    {
        bool isReadyToRegister = CheckConditionsToRegister();
        if (!isReadyToRegister)
        {
            return;
        }

        try
        {
            bool connectSuccessfully = await ClientObject.Instance.AuthenticateAsync(_inputEmail.text, _inputPassword.text, true);
            if (connectSuccessfully)
            {
                StartCoroutine(ChangeScene(MAIN_SCENE_NAME));
            }
        }
        catch (ApiResponseException ex)
        {
            SetErrorMessage(EXISTED_EMAIL);
        }
    }

    private IEnumerator ChangeScene(string sceneName)
    {
        _imageLoading.SetActive(true);
        yield return new WaitForSeconds(1);
        SceneManager.LoadSceneAsync(sceneName);
    }

    private bool CheckConditionsToRegister()
    {
        ResetVisuals();

        if (!IsValidEmail(_inputEmail.text))
        {
            SetErrorMessage(INVALID_EMAIL);
            _imageEmailInput.color = _erroredColor;
            return false;
        }

        if (!IsValidPassword(_inputPassword.text, out string errorMessage))
        {
            SetErrorMessage(errorMessage);
            _imagePasswordInput.color = _erroredColor;
            return false;
        }

        if (!_inputPassword.text.Equals(_inputConPassword.text))
        {
            SetErrorMessage(MISMATCH_PASSWORD_CONFIRMATION);
            _imageConPasswordInput.color = _erroredColor;
            return false;
        }

        if (!_toggleTos.isOn)
        {
            SetErrorMessage(NO_CHECK_TERMS_CONDITIONS);
            _textTos.color = _erroredColor;
            return false;
        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    private bool IsValidPassword(string password, out string feedbackMessage)
    {
        feedbackMessage = "";
        if (password.Length < 8)
        {
            feedbackMessage = "Password must be at least 8 characters long.";
            return false;
        }
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            feedbackMessage = "Password must contain at least one uppercase letter.";
            return false;
        }
        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            feedbackMessage = "Password must contain at least one lowercase letter.";
            return false;
        }
        if (!Regex.IsMatch(password, @"\d"))
        {
            feedbackMessage = "Password must contain at least one digit.";
            return false;
        }
        if (!Regex.IsMatch(password, @"[\W_]"))
        {
            feedbackMessage = "Password must contain at least one special character.";
            return false;
        }
        return true;
    }

    private void ResetVisuals()
    {
        _imageEmailInput.color = _normalColor;
        _imagePasswordInput.color = _normalColor;
        _imageConPasswordInput.color = _normalColor;
        _textTos.color = _normalColor;
        _textTos.alpha = 255;
        SetErrorMessage(string.Empty);
    }

    private void SetErrorMessage(string message)
    {
        _errorContainer.SetActive(!string.IsNullOrEmpty(message));
        _textErrorMessage.text = message;
    }
}
