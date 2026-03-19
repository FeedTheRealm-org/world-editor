using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FTR.Core.Common.Config;
using FTRShared.Runtime.Models;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Core.Repository
{
    [CreateAssetMenu(
        fileName = "WorldsRepository",
        menuName = "Scriptable Objects/Repository/WorldsRepository"
    )]
    public class WorldsRepository : ScriptableObject
    {
        [SerializeField]
        private Config config;

        [SerializeField]
        private Logging.Logger logger;

        private string extension => config.WorldFileExtension;
        private string saveDirectory => config.WorldDirectory;
        private string zoneFilePrefix => config.ZoneFilePrefix;

        public void SaveWorldZone(WorldData worldData)
        {
            try
            {
                EnsureWorldFolder(worldData.worldName);

                // Assign zoneId if not set
                if (worldData.zone_id <= 0)
                    worldData.zone_id = GetNextZoneId(worldData.worldName);

                string json = JsonUtility.ToJson(worldData, true);
                if (string.IsNullOrWhiteSpace(json) || json == "{}")
                {
                    logger.Log("No data to save.", Logging.LogType.Warning);
                    return;
                }

                string path = GetZonePath(worldData.worldName, worldData.zone_id);
                File.WriteAllText(path, json);
                logger.Log($"Saved zone {worldData.zone_id} in world '{worldData.worldName}'");
            }
            catch (Exception e)
            {
                logger.Log($"Error saving world: {e}", Logging.LogType.Error);
            }
        }

        public WorldData GetWorldZone(string worldName, int zoneId)
        {
            try
            {
                string path = GetZonePath(worldName, zoneId);
                if (!File.Exists(path))
                {
                    logger.Log($"New zone: {path}", Logging.LogType.Warning);
                    return null;
                }
                using FileStream fs = new(path, FileMode.Open);
                using StreamReader reader = new(fs);
                string dataToLoad = reader.ReadToEnd();
                WorldData worldData = JsonUtility.FromJson<WorldData>(dataToLoad);
                return worldData;
            }
            catch (Exception e)
            {
                logger.Log($"Error loading zone: {e}", Logging.LogType.Error);
                return null;
            }
        }

        public List<string> ListWorlds()
        {
            if (!Directory.Exists(saveDirectory))
                return new List<string>();
            var worldFolders = Directory.GetDirectories(saveDirectory);
            return worldFolders.Select(path => new DirectoryInfo(path).Name).ToList();
        }

        // Private helper methods

        private void EnsureWorldFolder(string worldName)
        {
            string path = GetWorldPath(worldName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private string GetWorldPath(string worldName)
        {
            return Path.Combine(saveDirectory, worldName);
        }

        private string GetZonePath(string worldName, int zoneId)
        {
            string fileName = $"{zoneFilePrefix}{zoneId}{extension}";
            return Path.Combine(GetWorldPath(worldName), fileName);
        }

        private int GetNextZoneId(string worldName)
        {
            string folder = GetWorldPath(worldName);
            if (!Directory.Exists(folder))
                return 1;

            var existingIds = Directory
                .GetFiles(folder, $"*{extension}")
                .Select(f => Path.GetFileNameWithoutExtension(f).Replace(zoneFilePrefix, ""))
                .Where(s => int.TryParse(s, out _))
                .Select(int.Parse)
                .ToHashSet();

            int id = 1;
            while (existingIds.Contains(id))
                id++;

            return id;
        }
    }
}
