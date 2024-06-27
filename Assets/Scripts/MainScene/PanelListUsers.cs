using Nakama;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PanelListUsers : MonoBehaviour
{
    [SerializeField] private UserStatusCard _userStatusCardPrefab;
    [SerializeField] private Transform _listUsersTransform;

    private ClientObject _clientObject;
    private List<string> ids = new List<string>()
    {
        "263178e6-f3f3-4fba-9277-c64d52579905",
        "30b9fec2-b9b6-4e4c-b483-347727078a4a",
        "e380eaa5-7bac-4283-9dbe-c5852d3fb31a",
        "0a44d1e6-dadc-40a0-a53f-ab72f71d4495"
    };

    private async void Start()
    {
        _clientObject = ClientObject.Instance;
        while (_clientObject.ThisUser == null)
        {
            await Task.Yield();
        }
        ids.Remove(_clientObject.ThisUser.Id);
        InvokeRepeating(nameof(RefreshListUsers), 1, 10);
    }

    private async void RefreshListUsers()
    {
        if (!_clientObject.Socket.IsConnected || !gameObject.activeInHierarchy)
            return;

        var users = await _clientObject.Client.GetUsersAsync(_clientObject.Session, ids);
        FillPanel(users.Users.ToList());
    }

    private void FillPanel(List<IApiUser> users)
    {
        foreach (Transform child in _listUsersTransform)
        {
            Destroy(child.gameObject);
        }

        foreach(var user in users)
        {
            UserStatusCard card = Instantiate(_userStatusCardPrefab, _listUsersTransform);
            card.Populate(user.Id, user.Username, user.DisplayName, user.Online);
        }
    }
}
