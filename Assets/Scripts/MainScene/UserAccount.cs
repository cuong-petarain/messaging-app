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
    [SerializeField] TMP_Text textDisplayName;

    private bool isInEditMode = false;

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
        textDisplayName.text = accountInfo.User.DisplayName;
    }
}
