using System.IO;
using UnityEngine;

namespace Utils
{
    public static class FileHandler
    {
        private static readonly string persistentFilePath = Application.streamingAssetsPath;

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
            File.Copy(sourceFilePath, targetFilePath, true);

            return Path.Combine(targetDirectory, Path.GetFileName(targetFilePath));
        }

        /// <summary>
        /// Deletes a file from the specified folder in StreamingAssets.
        /// </summary>
        public static void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            string fullPath = Path.Combine(persistentFilePath, filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
