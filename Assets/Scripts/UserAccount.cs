using Nakama;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;

public class UserAccount : MonoBehaviour
{
    [SerializeField] TMP_Text textEmail;
    [SerializeField] TMP_Text textUsername;
    [SerializeField] TMP_Text textDisplayName;
    [SerializeField] TMP_InputField inputName;
    [SerializeField] TMP_Text textButtonUpdate;

    private bool isInEditMode = false;
    public static Action<string, string> OnUpdateAccountInfoButtonPressed;

    private async void Start()
    {
        while (ClientObject.Instance.Socket == null || !ClientObject.Instance.Socket.IsConnected)
        {
            await Task.Yield();
        }
        IApiAccount account = await ClientObject.Instance.Client.GetAccountAsync(ClientObject.Instance.Session);
        ClientObject.Instance.SetUserInfo(account.User);
        GetUserProfile(account);
    }

    private void GetUserProfile(IApiAccount accountInfo)
    {
        textEmail.text = accountInfo.Email;
        textUsername.text = accountInfo.User.Username;
        textDisplayName.text = accountInfo.User.DisplayName;
        inputName.text = accountInfo.User.DisplayName;
    }

    public void UpdateAccountButtonClicked()
    {
        isInEditMode = !isInEditMode;
        if (isInEditMode)
        {
            textDisplayName.gameObject.SetActive(false);
            inputName.gameObject.SetActive(true);
            textButtonUpdate.text = "Update";
        }
        else
        {
            textDisplayName.text = inputName.text.Trim();
            textDisplayName.gameObject.SetActive(true);
            inputName.gameObject.SetActive(false);
            textButtonUpdate.text = "Edit";
            OnUpdateAccountInfoButtonPressed?.Invoke(textUsername.text, inputName.text.Trim());
        }
    }
}
