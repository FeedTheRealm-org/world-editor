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

        /// <summary>
        /// Saves a sprite file from source path to a folder in StreamingAssets.
        /// Returns the path where the file was saved.
        /// If fileName is null, uses the original filename from sourceFilePath.
        /// </summary>
        public static string SaveFile(
            string sourceFilePath,
            string targetDirectory = "Files",
            string fileName = null
        )
        {
            if (string.IsNullOrEmpty(sourceFilePath))
                return null;

            string targetDirPath = Path.Combine(persistentFilePath, targetDirectory);
            Directory.CreateDirectory(targetDirPath);

            string targetFilePath;
            if (!string.IsNullOrEmpty(fileName))
            {
                targetFilePath = Path.Combine(
                    targetDirPath,
                    fileName + Path.GetExtension(sourceFilePath)
                );
            }
            else
            {
                targetFilePath = Path.Combine(targetDirPath, Path.GetFileName(sourceFilePath));
            }
            // Avoid copying if the source and destination are the same file
            if (
                File.Exists(targetFilePath)
                && Path.GetFullPath(sourceFilePath) == Path.GetFullPath(targetFilePath)
            )
            {
                // Already exists and is the same file, do not copy
                return Path.Combine(targetDirectory, Path.GetFileName(targetFilePath));
            }
            FileBrowserHelpers.CopyFile(sourceFilePath, targetFilePath);

            return Path.Combine(targetDirectory, Path.GetFileName(targetFilePath));
        }

        /// <summary>
        /// Deletes a file from the specified folder in StreamingAssets.
        /// </summary>
        public static void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !FileBrowserHelpers.FileExists(filePath))
                return;

            FileBrowserHelpers.DeleteFile(filePath);
        }
    }
}
