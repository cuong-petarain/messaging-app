using Nakama;
using System.Collections;
using UnityEngine;
using TMPro;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PanelSettings : MonoBehaviour
{
    [SerializeField] private TMP_Text textDisplayName;
    [SerializeField] private PanelProfile _panelProfile;

    private void OnEnable()
    {
        PanelProfile.OnUserInfoChanged += UpdateSettings;
    }

    private void OnDisable()
    {
        PanelProfile.OnUserInfoChanged -= UpdateSettings;
    }

    private async void Start()
    {
        while (ClientObject.Instance.Socket == null || !ClientObject.Instance.Socket.IsConnected)
        {
            await Task.Yield();
        }
        IApiAccount account = await ClientObject.Instance.Client.GetAccountAsync(ClientObject.Instance.Session);
        ClientObject.Instance.SetUserInfo(account.User);
        GetUserProfile(account);
        _panelProfile.GetComponent<RectTransform>().DOLocalMoveX(Screen.width, 0);
    }

    private void GetUserProfile(IApiAccount accountInfo)
    {
        textDisplayName.text = accountInfo.User.DisplayName;
        _panelProfile.Populate(accountInfo);
    }

    public void OnViewProfileClicked()
    {
        _panelProfile.gameObject.SetActive(true);
        _panelProfile.GetComponent<RectTransform>().DOLocalMoveX(0, 0.25f);
    }

    public void OnBackFromProfileClicked()
    {
        _panelProfile.GetComponent<RectTransform>().DOLocalMoveX(Screen.width, 0.25f).OnComplete(() => _panelProfile.gameObject.SetActive(false));
    }

    public void OnLogoutButtonClicked()
    {
        ClientObject.Instance.Logout();
        StartCoroutine(ChangeScene("LandingScene"));
    }

    private void UpdateSettings(string displayName)
    {
        textDisplayName.text = displayName;
    }

    private IEnumerator ChangeScene(string sceneName)
    {
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadSceneAsync(sceneName);
    }
}
