using System.IO;
using SimpleFileBrowser;
using UnityEngine;

namespace Utils
{
    public static class CustomFileBrowser
    {
        private static readonly string persistentFilePath = Application.streamingAssetsPath;

        public static void ShowFilePickerDialog(
            FileBrowser.OnSuccess onSuccess,
            FileBrowser.OnCancel onCancel,
            string title = "Select Image",
            string loadButtonText = "Select",
            params string[] extensions
        )
        {
            if (extensions == null || extensions.Length == 0)
                extensions = new[] { ".png" };

            FileBrowser.SetFilters(false, new FileBrowser.Filter("Image Files", extensions));
            FileBrowser.SetDefaultFilter(extensions[0]);
            FileBrowser.ShowLoadDialog(
                onSuccess: onSuccess,
                onCancel: onCancel,
                pickMode: FileBrowser.PickMode.Files,
                allowMultiSelection: false,
                initialPath: null,
                initialFilename: null,
                title: title,
                loadButtonText: loadButtonText
            );
        }

        /// <summary>
        /// Loads a sprite from disk at the given path.
        /// </summary>
        public static Sprite LoadSpriteFromDisk(string path, bool loadFromPersistentPath = false)
        {
            if (loadFromPersistentPath)
                path = Path.Combine(persistentFilePath, path);

            if (!FileBrowserHelpers.FileExists(path))
                return null;

            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(path);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            if (!texture.LoadImage(bytes))
                return null;

            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }
    }
}
