using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AppManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup contactsPanel;
    [SerializeField] private CanvasGroup chatsPanel;
    [SerializeField] private CanvasGroup settingsPanel;
    [SerializeField] private FunctionButton buttonContacts;
    [SerializeField] private FunctionButton buttonChats;
    [SerializeField] private FunctionButton buttonSettings;

    private void Awake()
    {
        OnContactsButtonClicked();
    }

    public void OnContactsButtonClicked()
    {
        buttonContacts.ToggleActivation(true);
        buttonChats.ToggleActivation(false);
        buttonSettings.ToggleActivation(false);
        EnableCanvasGroup(contactsPanel);
        DisableCanvasGroup(chatsPanel);
        DisableCanvasGroup(settingsPanel);
    }

    public void OnChatsButtonClicked()
    {
        buttonContacts.ToggleActivation(false);
        buttonChats.ToggleActivation(true);
        buttonSettings.ToggleActivation(false);
        DisableCanvasGroup(contactsPanel);
        EnableCanvasGroup(chatsPanel);
        DisableCanvasGroup(settingsPanel);
    }

    public void OnSettingsButtonClicked()
    {
        buttonContacts.ToggleActivation(false);
        buttonChats.ToggleActivation(false);
        buttonSettings.ToggleActivation(true);
        DisableCanvasGroup(contactsPanel);
        DisableCanvasGroup(chatsPanel);
        EnableCanvasGroup(settingsPanel);
    }

    private void EnableCanvasGroup(CanvasGroup cg)
    {
        cg.alpha = 1.0f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private void DisableCanvasGroup(CanvasGroup cg)
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}
