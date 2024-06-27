using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using System;
using System.Threading.Tasks;

public class ClientObject : PersistentSingleton<ClientObject>
{
    private const string SessionPrefName = "nakama.session";
    private const string SingletonName = "ClientObject";

    public IClient Client { get; private set; }
    public ISocket Socket { get; private set; }
    public ISession Session { get; private set; }
    public IApiUser ThisUser { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        Init();
    }

    private void Init()
    {
        Client = new Client("http", "127.0.0.1", 7350, "defaultkey")
        {
            Timeout = 20,
#if UNITY_EDITOR
            Logger = new UnityLogger()
#endif
        };
        Socket = Client.NewSocket();
    }

    public async Task<bool> AuthenticateAsync(string email, string password, bool isRegister)
    {
        int atIndex = email.IndexOf("@");
        string username = email[..atIndex];
        // Restore session or create a new one.
        var authToken = PlayerPrefs.GetString(SessionPrefName);
        var session = Nakama.Session.Restore(authToken);
        var expiredDate = DateTime.UtcNow.AddDays(1);

        Init();
        if (session == null || session.HasExpired(expiredDate))
        {
            Session = await Client.AuthenticateEmailAsync(email, password, username, isRegister);
            if (Session.AuthToken != null)
            {
                PlayerPrefs.SetString(SessionPrefName, Session.AuthToken);
                await Socket.ConnectAsync(Session, true, 30);
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }
        }
        else
        {
            await Socket.ConnectAsync(Session, true, 30);
            return await Task.FromResult(true);
        }
        
    }

    public async void Logout()
    {
        SetUserInfo(null);
        await Socket.CloseAsync();
        await Client.SessionLogoutAsync(Session);
    }

    private void OnApplicationQuit()
    {
        Socket?.CloseAsync();
    }

    public async void UpdateAccountInfo(string username, string name)
    {
        await Client.UpdateAccountAsync(Session, username, name);
    }

    public void SetUserInfo(IApiUser userInfo)
    {
        ThisUser = userInfo;
    }
}
