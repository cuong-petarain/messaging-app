using Nakama;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UserAccount : MonoBehaviour
{
    [SerializeField] TMP_Text textId;
    [SerializeField] TMP_InputField inputUsername;
    [SerializeField] TMP_InputField inputName;

    public static Action<string, string> OnUpdateAccountInfoButtonPressed;

    private void OnEnable()
    {
        ClientObject.OnClientConnected += GetUserProfile;
    }

    private void OnDisable()
    {
        ClientObject.OnClientConnected -= GetUserProfile;
    }

    private void GetUserProfile(IApiAccount accountInfo)
    {
        textId.text = accountInfo.User.Id;
        inputUsername.text = accountInfo.User.Username;
        inputName.text = accountInfo.User.DisplayName;
    }

    public void UpdateAccountButtonClicked()
    {
        OnUpdateAccountInfoButtonPressed?.Invoke(inputUsername.text, inputName.text);
    }
}
