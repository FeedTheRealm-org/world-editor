using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.Utils;
using FTR.Core.Common.Config;
using FTRShared.Runtime.Models;
using UnityEngine;
using VContainer.Unity;

namespace FeedTheRealm.Core.Repository
{
    public class ZonesRepository : IInitializable
    {
        private Config config;
        private Logging.Logger logger;

        private string extension => config.WorldFileExtension;
        private string worldsDirectory => config.WorldDirectory;
        private string zoneFilePrefix => config.ZoneFilePrefix;

        public ZonesRepository(Config config, Logging.Logger logger)
        {
            this.config = config;
            this.logger = logger;
        }

        // IInitializable requiers this method, but we don't need to do anything on initialization for this repository
        // We implement this interface just to ensure that the repository is created when registered.
        public void Initialize() { }

        public void SaveZoneData(string worldDataPath, ZoneData zone)
        {
            string path = GetZonePath(worldDataPath, zone.zoneId);
            if (FileSystemHandler.TryWriteJson(path, zone, logger))
                logger.Log($"Saved zone {zone.zoneId} to '{path}'");
        }

        public ZoneData GetZoneData(string worldName, int zoneId)
        {
            try
            {
                if (string.IsNullOrEmpty(worldName))
                    return null;
                string path = GetZonePath(worldName, zoneId);
                if (!File.Exists(path))
                    return null;
                using FileStream fs = new(path, FileMode.Open);
                using StreamReader reader = new(fs);
                string dataToLoad = reader.ReadToEnd();
                ZoneData worldData = JsonUtility.FromJson<ZoneData>(dataToLoad);
                return worldData;
            }
            catch (Exception e)
            {
                logger.Log($"Error loading zone: {e}", Logging.LogType.Error);
                return null;
            }
        }

        private string GetZonePath(string worldName, int zoneId)
        {
            string fileName = $"{zoneFilePrefix}{zoneId}{extension}";
            return Path.Combine(GetWorldPath(worldName), fileName);
        }

        public List<int> ListZones(string worldName)
        {
            if (!Directory.Exists(GetWorldPath(worldName)))
                return new List<int>();

            return Directory
                .GetFiles(GetWorldPath(worldName), $"*{extension}")
                .Select(f => Path.GetFileNameWithoutExtension(f).Replace(zoneFilePrefix, ""))
                .Where(s => int.TryParse(s, out _))
                .Select(int.Parse)
                .OrderBy(id => id)
                .ToList();
        }

        public int GetNextZoneId(string worldName)
        {
            var zones = ListZones(worldName);
            return zones.Count > 0 ? zones.Max() + 1 : 0;
        }

        private string GetWorldPath(string worldName)
        {
            return Path.Combine(worldsDirectory, worldName);
        }
    }
}
