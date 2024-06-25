using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingManager : MonoBehaviour
{
    [SerializeField] private GameObject _panelRegister;
    [SerializeField] private GameObject _panelLogin;

    public void OnButtonGoToLogin()
    {
        _panelRegister.SetActive(false);
        _panelLogin.SetActive(true);
    }

    public void OnButtonGoToRegister()
    {
        _panelRegister.SetActive(true);
        _panelLogin.SetActive(false);
    }
}
