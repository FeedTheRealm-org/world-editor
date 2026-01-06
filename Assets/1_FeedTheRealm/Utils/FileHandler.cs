using System.IO;
using SimpleFileBrowser;
using UnityEngine;

namespace Utils
{
    public static class FileHandler
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
        public static Sprite LoadSpriteFromDisk(string path)
        {
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

        /// <summary>
        /// Saves a sprite file from source path to the Items folder in StreamingAssets.
        /// Returns the sprite ID used for the saved file.
        /// </summary>
        public static string SaveFile(
            string sourceFilePath,
            string fileId,
            string fileExtension = ".png",
            string targetDirectory = "Files"
        )
        {
            if (string.IsNullOrEmpty(sourceFilePath))
                return null;

            string targetDirPath = Path.Combine(persistentFilePath, targetDirectory);
            Directory.CreateDirectory(targetDirPath);

            if (string.IsNullOrEmpty(fileId))
                return null;

            string targetFilePath = Path.Combine(targetDirPath, fileId + fileExtension);
            FileBrowserHelpers.CopyFile(sourceFilePath, targetFilePath);

            return fileId;
        }

        public static bool DeleteFile(
            string fileId,
            string fileExtension = ".png",
            string targetDirectory = "Files"
        )
        {
            if (string.IsNullOrEmpty(fileId))
                return false;

            string targetFilePath = Path.Combine(
                persistentFilePath,
                targetDirectory,
                fileId + fileExtension
            );

            if (!FileBrowserHelpers.FileExists(targetFilePath))
                return false;

            FileBrowserHelpers.DeleteFile(targetFilePath);
            return true;
        }

        /// <summary>
        /// Gets the expected path for an image file in StreamingAssets given an image ID.
        /// </summary>
        public static string GetSpriteFilePath(
            string spriteId,
            string fileExtension = ".png",
            string targetDirectory = "Sprites"
        )
        {
            if (string.IsNullOrEmpty(spriteId))
                return null;

            return Path.Combine(persistentFilePath, targetDirectory, spriteId + fileExtension);
        }
    }
}
