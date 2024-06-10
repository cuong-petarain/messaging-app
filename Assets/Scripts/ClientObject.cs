using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using System;
using System.Threading.Tasks;

public class ClientObject : MonoBehaviour
{
    private static ClientObject instance;
    public static ClientObject Instance => instance;

    private const string SessionTokenKey = "nksession";
    private const string UdidKey = "udid";
    private string deviceId;

    public string thisUserId { get; set; }

    private IClient client;
    public IClient Client => client;
    private ISocket socket;
    public ISocket Socket => socket;
    private ISession session;
    public ISession Session => session;

    public static Action<IApiAccount> OnClientConnected;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void OnEnable()
    {
        UserAccount.OnUpdateAccountInfoButtonPressed += UpdateAccountInfo;
    }

    private void OnDisable()
    {
        UserAccount.OnUpdateAccountInfoButtonPressed -= UpdateAccountInfo;
        socket.ReceivedNotification -= PanelMessage.Instance.HandleNotification;
    }

    // Start is called before the first frame update
    private async void Start()
    {
        await Connect();
        var account = await client.GetAccountAsync(session);
        thisUserId = account.User.Id;
        OnClientConnected?.Invoke(account);
    }

    private async Task Connect()
    {
        // client
        client = new Client("http", "127.0.0.1", 7350, "defaultkey");
        client.Timeout = 10;
        var logger = new UnityLogger();
        client.Logger = logger;
        // session
        //deviceId = SystemInfo.deviceUniqueIdentifier;
        //if (deviceId == SystemInfo.unsupportedIdentifier)
        //    deviceId = System.Guid.NewGuid().ToString();
        deviceId = Application.companyName + Application.productName;
        Dictionary<string, string> vars = new Dictionary<string, string>();
        vars["DeviceOS"] = SystemInfo.operatingSystem;
        vars["DeviceModel"] = SystemInfo.deviceModel;
        vars["GameVersion"] = Application.version;
        session = await client.AuthenticateDeviceAsync(deviceId, null, true, vars);
        PlayerPrefs.SetString("deviceId", deviceId);
        PlayerPrefs.SetString("nakama.authToken", session.AuthToken);
        PlayerPrefs.SetString("nakama.refreshToken", session.RefreshToken);

        // socket
        bool appearOnline = true;
        int connectionTimeout = 30;
        socket = client.NewSocket();
        socket.ReceivedNotification += PanelMessage.Instance.HandleNotification;
        await socket.ConnectAsync(session, appearOnline, connectionTimeout);
    }

    public async void Logout()
    {
        await client.SessionLogoutAsync(session);
    }

    private async void UpdateAccountInfo(string username, string name)
    {
        await client.UpdateAccountAsync(session, username, name);
    }

    private async void CheckSessionExpiration()
    {
        // Check whether a session has expired or is close to expiry.
        if (session.IsExpired || session.HasExpired(DateTime.UtcNow.AddDays(1)))
        {
            try
            {
                // Attempt to refresh the existing session.
                session = await client.SessionRefreshAsync(session);
            }
            catch (ApiResponseException)
            {
                // Couldn't refresh the session so reauthenticate.
                session = await client.AuthenticateDeviceAsync(deviceId);
                PlayerPrefs.SetString("nakama.refreshToken", session.RefreshToken);
            }

            PlayerPrefs.SetString("nakama.authToken", session.AuthToken);
        }
    }
}
