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
    [Serializable]
    public class SerializedStructureData
    {
        public List<StructureData> structures = new();
    }

    public class ModelsRepository
    {
        [Inject]
        private Config config;

        [Inject]
        private Logging.Logger logger;

        private Dictionary<string, StructureData> modelsData;

        public ModelsRepository(Config config, Logging.Logger logger)
        {
            this.config = config;
            this.logger = logger;
            {
                if (!File.Exists(config.ModelsDataFile))
                    GenerateDefaultFile();

                modelsData = LoadFromDisk().ToDictionary(m => m.id, m => m);
                logger.Log(
                    $"ModelsRepository loaded {modelsData.Count} models.",
                    Logging.LogType.Info
                );
            }
        }

        public StructureData GetStructureData(string id)
        {
            if (modelsData.TryGetValue(id, out var data))
                return data;

            logger.Log($"Structure '{id}' not found in repository.", Logging.LogType.Error);
            return null;
        }

        public Dictionary<string, StructureData> GetModelsData() => modelsData;

        private void GenerateDefaultFile()
        {
            string dataFileDir = Path.GetDirectoryName(config.ModelsDataFile);
            if (!string.IsNullOrEmpty(dataFileDir))
                Directory.CreateDirectory(dataFileDir);

            string modelsDir = config.ModelsDirectory;
            if (!Directory.Exists(modelsDir))
            {
                logger.Log(
                    $"Models directory not found at '{modelsDir}'. Creating it — add your .glb files there.",
                    Logging.LogType.Warning
                );
                Directory.CreateDirectory(modelsDir);
                WriteToFile(new List<StructureData>());
                return;
            }

            logger.Log($"Scanning models directory: {modelsDir}", Logging.LogType.Info);

            var models = Directory
                .GetFiles(modelsDir, "*.glb", SearchOption.AllDirectories)
                .Select(path => new StructureData(
                    id: Guid.NewGuid().ToString(),
                    structureName: Path.GetFileNameWithoutExtension(path),
                    size: Vector3.one,
                    rotation: Vector3.zero,
                    fileName: Path.GetFileName(path)
                ))
                .ToList();

            if (models.Count == 0)
                logger.Log("No .glb models found in models directory.", Logging.LogType.Warning);

            WriteToFile(models);
            logger.Log($"Generated models file with {models.Count} models.", Logging.LogType.Info);
        }

        private void WriteToFile(List<StructureData> models)
        {
            try
            {
                string json = JsonUtility.ToJson(
                    new SerializedStructureData { structures = models },
                    true
                );
                File.WriteAllText(config.ModelsDataFile, json);
            }
            catch (Exception ex)
            {
                logger.Log($"Failed to write models file: {ex.Message}", Logging.LogType.Error);
            }
        }

        private List<StructureData> LoadFromDisk()
        {
            try
            {
                string json = File.ReadAllText(config.ModelsDataFile);
                return JsonUtility.FromJson<SerializedStructureData>(json)?.structures
                    ?? new List<StructureData>();
            }
            catch (Exception ex)
            {
                logger.Log($"Error loading models data: {ex.Message}", Logging.LogType.Error);
                return new List<StructureData>();
            }
        }
    }
}
