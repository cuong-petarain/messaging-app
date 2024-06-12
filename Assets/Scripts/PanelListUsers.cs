using Nakama;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PanelListUsers : MonoBehaviour
{
    [SerializeField] private UserStatusCard _userStatusCardPrefab;
    private ClientObject _clientObject;

    private List<string> ids = new List<string>()
    {
        "7f002a87-1a52-42a9-b61f-2bbd97dd3bae",
        "e50c2e14-a1dc-4f6e-922b-2a044ea7d794",
        "f836ddf1-c315-4774-97c1-cd81193b8260",
        "a0941555-ed4f-4403-a0fa-2de25022200b",
        "48c0cbd0-3e93-4a17-83cd-ed5aaf2a664a"
    };

    private async void Start()
    {
        _clientObject = ClientObject.Instance;
        while (_clientObject.ThisUser == null)
        {
            await Task.Yield();
        }
        ids.Remove(_clientObject.ThisUser.Id);
        InvokeRepeating(nameof(RefreshListUsers), 2, 5);
    }

    private async void RefreshListUsers()
    {
        if (!_clientObject.Socket.IsConnected)
            return;

        var users = await _clientObject.Client.GetUsersAsync(_clientObject.Session, ids);
        FillPanel(users.Users.ToList());
    }

    private void FillPanel(List<IApiUser> users)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach(var user in users)
        {
            UserStatusCard card = Instantiate(_userStatusCardPrefab, transform);
            card.Populate(user.Id, user.DisplayName, user.Online);
        }
    }
}
