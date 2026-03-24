using System;
using System.IO;
using FTR.Core.Common.Config;
using FTRShared.Runtime.Models;
using VContainer.Unity;

namespace FeedTheRealm.Core.Repository
{
    public class CreatablesRepository : IInitializable
    {
        private readonly Config config;
        private readonly Logging.Logger logger;
        private string worldsDirectory => config.WorldDirectory;
        private string CreatablesFileName => config.CreatablesFileName;

        public CreatablesRepository(Config config, Logging.Logger logger)
        {
            this.config = config;
            this.logger = logger;
        }

        public void Initialize() { }

        public void SaveCreatables(string worldId, CreatablesData creatables)
        {
            string path = GetCreatablesPath(worldId);
            if (FileSystemHelper.TryWriteJson(path, creatables, logger))
                logger.Log($"Saved creatables to '{path}'");
        }

        public CreatablesData GetCreatables(string worldId)
        {
            string path = GetCreatablesPath(worldId);
            if (!File.Exists(path))
            {
                logger.Log(
                    $"No creatables found at '{path}', returning empty.",
                    Logging.LogType.Warning
                );
                return new CreatablesData();
            }
            return FileSystemHelper.TryReadJson<CreatablesData>(path, logger);
        }

        private string GetCreatablesPath(string worldId) =>
            Path.Combine(worldsDirectory, worldId, CreatablesFileName);
    }
}
