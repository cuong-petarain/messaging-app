using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SimpleFileBrowser;
using System.IO;
using DG.Tweening.Plugins.Core.PathCore;

public class AvatarImageHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _imageAvatar;

    public void OnPointerClick(PointerEventData eventData)
    {
        OpenFilesBrowser();
    }

    private void OpenFilesBrowser()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Custom Files", ".jpg", ".jpeg", ".png", ".gif"));
        FileBrowser.SetDefaultFilter(".png");
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe", ".pdf");
        FileBrowser.AddQuickLink("Users", "C:\\Users\\PC\\Pictures\\Images");
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    private IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Open File", "Open");

        if (FileBrowser.Success)
        {
            var filePath = FileBrowser.Result[0];
            if (File.Exists(filePath))
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(fileData))
                {
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    _imageAvatar.sprite = sprite;
                }
                else
                {
                    Debug.LogError("Failed to load texture from " + filePath);
                }
            }
        }
    }
}
