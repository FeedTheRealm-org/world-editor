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

        private string CreatablesFileName => config.CreatablesFileName;

        public CreatablesRepository(Config config, Logging.Logger logger)
        {
            this.config = config;
            this.logger = logger;
        }

        public void Initialize() { }

        public void SaveCreatables(string worldDataDirectory, WorldCreatables creatables)
        {
            string path = GetCreatablesPath(worldDataDirectory);
            if (FileSystemHelper.TryWriteJson(path, creatables, logger))
                logger.Log($"Saved creatables to '{path}'");
        }

        public WorldCreatables GetCreatables(string worldDataDirectory)
        {
            string path = GetCreatablesPath(worldDataDirectory);
            if (!File.Exists(path))
            {
                logger.Log(
                    $"No creatables found at '{path}', returning empty.",
                    Logging.LogType.Warning
                );
                return new WorldCreatables();
            }
            return FileSystemHelper.TryReadJson<WorldCreatables>(path, logger);
        }

        private string GetCreatablesPath(string worldDataDirectory) =>
            Path.Combine(worldDataDirectory, CreatablesFileName);
    }
}
