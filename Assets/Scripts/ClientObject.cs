using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using System;
using System.Threading.Tasks;

public class ClientObject : MonoBehaviour
{
    public PanelMessage panelMessage;

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
        socket.ReceivedNotification -= panelMessage.HandleNotification;
    }

    // Start is called before the first frame update
    private void Start()
    {
        Connect();
    }

    private async void Connect()
    {
        // client
        client = new Client("http", "127.0.0.1", 7350, "defaultkey");
        client.Timeout = 10;
        var logger = new UnityLogger();
        client.Logger = logger;
        
        // session
        deviceId = Application.companyName + Application.productName;
        session = await client.AuthenticateDeviceAsync(deviceId);

        // socket
        bool appearOnline = true;
        int connectionTimeout = 30;
        socket = client.NewSocket();
        socket.ReceivedNotification += panelMessage.HandleNotification;
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
}
