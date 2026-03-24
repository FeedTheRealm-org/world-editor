using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FeedTheRealm.Core.DataPersistence;
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
            if (FileSystemHelper.TryWriteJson(path, zone, logger))
                logger.Log($"Saved zone {zone.zoneId} to '{path}'");
        }

        public ZoneData GetZoneData(string worldId, int zoneId)
        {
            try
            {
                string path = GetZonePath(worldId, zoneId);
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

        public List<int> ListZones(string worldId)
        {
            if (!Directory.Exists(GetWorldPath(worldId)))
                return new List<int>();

            return Directory
                .GetFiles(GetWorldPath(worldId), $"*{extension}")
                .Select(f => Path.GetFileNameWithoutExtension(f).Replace(zoneFilePrefix, ""))
                .Where(s => int.TryParse(s, out _))
                .Select(int.Parse)
                .OrderBy(id => id)
                .ToList();
        }

        private string GetWorldPath(string worldId)
        {
            return Path.Combine(worldsDirectory, worldId);
        }
    }
}
