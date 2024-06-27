using System;
using System.Collections;
using SimpleFileBrowser;
using UnityEngine;

public class OpenFilePanel : MonoBehaviour
{
    public static string CurrentPath;

    public Guid slotId
    {
        get; set;
    }

    public void OpenFilesBrowser()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Custom Files", ".cif", ".pdb", ".sdf"),
            new FileBrowser.Filter("SDF Files", ".sdf"));
        FileBrowser.SetDefaultFilter(".cif");
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");
        FileBrowser.AddQuickLink("Users", "C:\\Users\\PC\\Pictures\\Images");
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    private IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Open File", "Open");

        if (FileBrowser.Success)
        {
            var filePath = FileBrowser.Result[0];
            CurrentPath = filePath;
        }
    }

    
}
