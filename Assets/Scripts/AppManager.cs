using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AppManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup contactsPanel;
    [SerializeField] private CanvasGroup chatsPanel;
    [SerializeField] private CanvasGroup settingsPanel;
    [SerializeField] private Image imageButtonContacts;
    [SerializeField] private Image imageButtonChats;
    [SerializeField] private Image imageButtonSettings;

    private void Awake()
    {
        OnContactsButtonClicked();
    }

    public void OnContactsButtonClicked()
    {
        imageButtonContacts.enabled = true;
        imageButtonChats.enabled = false;
        imageButtonSettings.enabled = false;
        EnableCanvasGroup(contactsPanel);
        DisableCanvasGroup(chatsPanel);
        DisableCanvasGroup(settingsPanel);
    }

    public void OnChatsButtonClicked()
    {
        imageButtonContacts.enabled = false;;
        imageButtonChats.enabled = true;
        imageButtonSettings.enabled = false;
        DisableCanvasGroup(contactsPanel);
        EnableCanvasGroup(chatsPanel);
        DisableCanvasGroup(settingsPanel);
    }

    public void OnSettingsButtonClicked()
    {
        imageButtonContacts.enabled = false;
        imageButtonChats.enabled = false;
        imageButtonSettings.enabled = true;
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
