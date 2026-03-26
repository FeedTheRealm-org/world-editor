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

        public void SaveCreatables(string worldName, CreatablesData creatables)
        {
            string path = GetCreatablesPath(worldName);
            if (FileSystemHelper.TryWriteJson(path, creatables, logger))
                logger.Log($"Saved creatables to '{path}'");
        }

        public CreatablesData GetCreatables(string worldName)
        {
            try
            {
                string path = GetCreatablesPath(worldName);
                if (!File.Exists(path))
                {
                    logger.Log(
                        $"No creatables found at '{path}', returning empty.",
                        Logging.LogType.Warning
                    );
                    return null;
                }
                return FileSystemHelper.TryReadJson<CreatablesData>(path, logger);
            }
            catch (Exception e)
            {
                logger.Log($"Error loading creatables: {e}", Logging.LogType.Error);
                return null;
            }
        }

        private string GetCreatablesPath(string worldName) =>
            Path.Combine(worldsDirectory, worldName, CreatablesFileName);
    }
}
