using System;
using System.IO;
using UnityEngine;

namespace FeedTheRealm.Core.Utils
{
    public static class FileSystemHandler
    {
        public static void EnsureDirectory(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? path);
        }

        public static bool TryWriteJson<T>(string path, T data, Logging.Logger logger)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                if (string.IsNullOrWhiteSpace(json) || json == "{}")
                {
                    logger.Log("No data to save.", Logging.LogType.Warning);
                    return false;
                }
                EnsureDirectory(path);
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception e)
            {
                logger.Log($"Error writing to '{path}': {e}", Logging.LogType.Error);
                return false;
            }
        }

        public static T TryReadJson<T>(string path, Logging.Logger logger)
            where T : new()
        {
            try
            {
                if (!File.Exists(path))
                    return new T();
                return JsonUtility.FromJson<T>(File.ReadAllText(path)) ?? new T();
            }
            catch (Exception e)
            {
                logger.Log($"Error reading from '{path}': {e}", Logging.LogType.Error);
                return default;
            }
        }

        public static string SaveSprite(string sourcePath, string spritesDirectory, string id)
        {
            try
            {
                if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
                    return null;

                Directory.CreateDirectory(spritesDirectory);
                string fileExtension = Path.GetExtension(sourcePath);
                string destPath = Path.Combine(spritesDirectory, id + fileExtension);
                File.Copy(sourcePath, destPath, overwrite: true);
                Debug.Log($"Sprite saved to '{destPath}'");
                return id + fileExtension;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save sprite: {e.Message}");
                return null;
            }
        }

        public static void DeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error deleting file at '{path}': {e}");
            }
        }
    }
}
