using Nakama;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Threading.Tasks;

public class UserAccount : MonoBehaviour
{
    [SerializeField] TMP_Text textId;
    [SerializeField] TMP_InputField inputUsername;
    [SerializeField] TMP_InputField inputName;

    public static Action<string, string> OnUpdateAccountInfoButtonPressed;

    private async void Start()
    {
        while (ClientObject.Instance.Socket == null || !ClientObject.Instance.Socket.IsConnected)
        {
            await Task.Yield();
        }
        var account = await ClientObject.Instance.Client.GetAccountAsync(ClientObject.Instance.Session);
        ClientObject.Instance.thisUserId = account.User.Id;
        GetUserProfile(account);
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
