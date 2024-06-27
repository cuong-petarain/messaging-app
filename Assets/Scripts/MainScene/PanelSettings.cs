using Nakama;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Satori;

public class PanelSettings : MonoBehaviour
{
    [SerializeField] TMP_Text textDisplayName;

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

    public void OnLogoutButtonClicked()
    {
        ClientObject.Instance.Logout();
        StartCoroutine(ChangeScene("LandingScene"));
    }

    private IEnumerator ChangeScene(string sceneName)
    {
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadSceneAsync(sceneName);
    }
}
