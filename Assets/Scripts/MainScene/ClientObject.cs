using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using System;
using System.Threading.Tasks;

public class ClientObject : MonoBehaviour
{
    private const string SessionPrefName = "nakama.session";
    private const string SingletonName = "ClientObject";

    private static readonly object Lock = new object();
    private static ClientObject _instance;

    public static ClientObject Instance
    {
        get
        {
            lock (Lock)
            {
                if (_instance != null)
                    return _instance;
                var go = GameObject.Find(SingletonName) ?? new GameObject(SingletonName);

                if (go.GetComponent<ClientObject>() == null)
                {
                    go.AddComponent<ClientObject>();
                }
                DontDestroyOnLoad(go);
                _instance = go.GetComponent<ClientObject>();
                return _instance;
            }
        }
    }

    public IClient Client { get; }
    public ISocket Socket { get; }
    public ISession Session { get; private set; }
    public IApiUser ThisUser { get; private set; }

    public static Action<IApiAccount> OnClientConnected;

    private ClientObject()
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

    private Task<ISession> AuthentqweicateAsync(string email, string password)
    {
        // Modify to fit the authentication strategy you want within your game.
        // EXAMPLE:
        const string deviceIdPrefName = "deviceid";
        var deviceId = PlayerPrefs.GetString(deviceIdPrefName, SystemInfo.deviceUniqueIdentifier);
#if UNITY_EDITOR
        Debug.LogFormat("Device id: {0}", deviceId);
#endif
        // With device IDs save it locally in case of OS updates which can change the value on device.
        PlayerPrefs.SetString(deviceIdPrefName, deviceId);
        return Client.AuthenticateDeviceAsync(deviceId);
    }

    public async Task<bool> AuthenticateAsync(string email, string password, bool isRegister)
    {
        // Restore session or create a new one.
        var authToken = PlayerPrefs.GetString(SessionPrefName);
        var session = Nakama.Session.Restore(authToken);
        var expiredDate = DateTime.UtcNow.AddDays(1);

        if (session == null || session.HasExpired(expiredDate))
        {
            Session = await Client.AuthenticateEmailAsync(email, password, email, isRegister);
            if (Session.AuthToken != null)
            {
                PlayerPrefs.SetString(SessionPrefName, Session.AuthToken);
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }
        }
        else
        {
            return await Task.FromResult(true);
        }
        
    }

    private void OnApplicationQuit() => Socket?.CloseAsync();

    public async void UpdateAccountInfo(string username, string name)
    {
        await Client.UpdateAccountAsync(Session, username, name);
    }

    public void SetUserInfo(IApiUser userInfo)
    {
        ThisUser = userInfo;
    }
}
