using System;
using System.Collections;
using UnityEngine;
#if !UNITY_EDITOR
using SimpleFileBrowser;
#endif

namespace FeedTheRealm.Utils {
  /// <summary>
  /// Centralized helper for selecting sprite files both in the editor and in builds.
  /// In the editor, it uses UnityEditor.EditorUtility; at runtime, it uses SimpleFileBrowser.
  /// </summary>
  public static class SpriteFilePicker {
    /// <summary>
    /// Opens a panel to select an image file and returns the absolute path
    /// or null if the user cancels. In the editor, it is synchronous; at runtime, the coroutine variant is used.
    /// </summary>
    public static string OpenSpriteFilePanel(string title = "Select Sprite") {
#if UNITY_EDITOR
      // Editor: uses Unity's native panel synchronously
      string startDir = Application.dataPath;
      return UnityEditor.EditorUtility.OpenFilePanel(title, startDir, "png,jpg,jpeg");
#else
      Debug.LogWarning("SpriteFilePicker.OpenSpriteFilePanel: use WaitForSpriteFilePanel at runtime (this call will return null).");
      return null;
#endif
    }

    /// <summary>
    /// Coroutine variant that works both in the editor and at runtime.
    /// Calls onSelected with the selected path or with null if canceled.
    /// </summary>
    public static IEnumerator WaitForSpriteFilePanel(string title, Action<string> onSelected) {
  #if UNITY_EDITOR
      // In the editor, we reuse the synchronous panel to simplify
      string selected = OpenSpriteFilePanel(title);
      onSelected?.Invoke(selected);
      yield break;
#else
      // Configure filters for images
      FileBrowser.SetFilters(true,
        new FileBrowser.Filter("Images", ".png", ".jpg", ".jpeg", ".bmp"));

      // Optional: default filter
      FileBrowser.SetDefaultFilter(".png");

      // Show single file load dialog
      yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files,
        false,
        null,
        null,
        title,
        "Select");

      if (FileBrowser.Success && FileBrowser.Result != null && FileBrowser.Result.Length > 0)
        onSelected?.Invoke(FileBrowser.Result[0]);
      else
        onSelected?.Invoke(null);
#endif
    }
  }
}
