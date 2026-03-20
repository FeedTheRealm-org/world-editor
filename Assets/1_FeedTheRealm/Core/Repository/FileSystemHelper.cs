using System;
using System.IO;
using UnityEngine;

namespace FeedTheRealm.Core.Repository
{
    static class FileSystemHelper
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
    }
}
