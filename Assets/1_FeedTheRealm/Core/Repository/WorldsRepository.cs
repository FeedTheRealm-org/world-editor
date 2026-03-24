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
            if (string.IsNullOrEmpty(worldData.worldId))
                throw new ArgumentException("WorldData must have a worldId before saving.");

            string path = GetWorldDataFilePath(worldData.worldId);
            if (FileSystemHelper.TryWriteJson(path, worldData, logger))
                logger.Log($"Saved world data to '{path}'");
        }

        public WorldData GetWorldData(string worldId)
        {
            try
            {
                string path = GetWorldDataFilePath(worldId);
                if (!File.Exists(path))
                {
                    logger.Log(
                        $"No world data found at '{path}', returning empty.",
                        Logging.LogType.Warning
                    );
                    return null;
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

        /// <summary>
        ///  Lists all worlds by their worldId (which is the name of the folder where their data is stored).
        ///  This is used to populate the "Load World" menu.
        /// </summary>
        public List<string> ListWorlds()
        {
            if (!Directory.Exists(worldsDirectory))
                return new List<string>();

            return Directory
                .GetDirectories(worldsDirectory)
                .Select(path => new DirectoryInfo(path).Name)
                .ToList();
        }

        private string GetWorldDataFilePath(string worldId) =>
            Path.Combine(worldsDirectory, worldId, worldDataFileName);
    }
}
