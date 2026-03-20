using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FTR.Core.Common.Config;
using FTRShared.Runtime.Models;
using UnityEngine;
using VContainer.Unity;

namespace FeedTheRealm.Core.Repository
{
    public class WorldsRepository : IInitializable
    {
        private readonly Config config;
        private readonly Logging.Logger logger;

        private string worldsDirectory => config.WorldDirectory;
        private string worldDataFileName => config.WorldDataFilename;

        public WorldsRepository(Config config, Logging.Logger logger)
        {
            this.config = config;
            this.logger = logger;
        }

        public void Initialize() { }

        public void SaveWorldData(WorldData worldData)
        {
            if (string.IsNullOrEmpty(worldData.worldDataDirectory))
                worldData.worldDataDirectory = Path.Combine(
                    worldsDirectory,
                    Guid.NewGuid().ToString()
                );

            string path = GetWorldDataFilePath(worldData.worldDataDirectory);
            if (FileSystemHelper.TryWriteJson(path, worldData, logger))
                logger.Log($"Saved world data to '{path}'");
        }

        public WorldData GetWorldData(string worldDataDirectory)
        {
            try
            {
                string path = GetWorldDataFilePath(worldDataDirectory);
                if (!File.Exists(path))
                {
                    logger.Log(
                        $"No world data found at '{path}', returning empty.",
                        Logging.LogType.Warning
                    );
                    return new WorldData();
                }

                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<WorldData>(json) ?? new WorldData();
            }
            catch (Exception e)
            {
                logger.Log($"Error loading world data: {e}", Logging.LogType.Error);
                return null;
            }
        }

        public List<string> ListWorlds()
        {
            if (!Directory.Exists(worldsDirectory))
                return new List<string>();

            return Directory
                .GetDirectories(worldsDirectory)
                .Select(path => new DirectoryInfo(path).Name)
                .ToList();
        }

        private string GetWorldDataFilePath(string worldDataDirectory) =>
            Path.Combine(worldDataDirectory, worldDataFileName);
    }
}
