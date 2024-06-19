using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup contactsPanel;
    [SerializeField] private CanvasGroup chatsPanel;
    [SerializeField] private CanvasGroup settingsPanel;

    private void Awake()
    {
        OnContactsButtonClicked();
    }

    public void OnContactsButtonClicked()
    {
        EnableCanvasGroup(contactsPanel);
        DisableCanvasGroup(chatsPanel);
        DisableCanvasGroup(settingsPanel);
    }

    public void OnChatsButtonClicked()
    {
        DisableCanvasGroup(contactsPanel);
        EnableCanvasGroup(chatsPanel);
        DisableCanvasGroup(settingsPanel);
    }

    public void OnSettingsButtonClicked()
    {
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
