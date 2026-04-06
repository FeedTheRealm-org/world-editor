using System.IO;
using SimpleFileBrowser;
using UnityEngine;

namespace Utils
{
    public static class CustomFileBrowser
    {
        private static readonly string persistentFilePath = Application.streamingAssetsPath;

        /// <summary>
        /// Shows a file browser dialog to select a file.
        /// </summary>
        public static void ShowFilePickerDialog(
            FileBrowser.OnSuccess onSuccess,
            FileBrowser.OnCancel onCancel,
            string title = "Select Image",
            string loadButtonText = "Select",
            string extensionType = ".png"
        )
        {
            FileBrowser.SetFilters(
                false,
                new FileBrowser.Filter($"{extensionType} Images", extensionType)
            );
            FileBrowser.SetDefaultFilter(extensionType);
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
