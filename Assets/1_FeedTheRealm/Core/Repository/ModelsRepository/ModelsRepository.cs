using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FTR.Core.Common.Config;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Core.Repository
{
    [Serializable]
    public class SerializedStructureData
    {
        public List<StructureData> structures = new();
    }

    [CreateAssetMenu(
        fileName = "ModelsRepository",
        menuName = "Scriptable Objects/Repository/ModelsRepository"
    )]
    public class ModelsRepository : ScriptableObject
    {
        [SerializeField]
        private Config config;

        [SerializeField]
        private Logging.Logger logger;
        private bool isInitialized = false;
        private Dictionary<string, StructureData> modelsData;

        public void Initialize()
        {
            if (config.ForceReinitialize)
                isInitialized = false;

            if (isInitialized)
                return;

            if (!File.Exists(config.ModelsDataFile))
                GenerateDefaultFile();

            var modelsList = LoadFromDisk();
            modelsData = modelsList.ToDictionary(m => m.id, m => m);
            logger.Log($"ModelsRepository loaded {modelsData.Count} models", Logging.LogType.Info);
            isInitialized = true;
        }

        public StructureData GetStructureData(string structureName)
        {
            if (modelsData.TryGetValue(structureName, out var structureData))
                return structureData;
            else
                logger.Log(
                    $"Structure {structureName} not found in repository",
                    Logging.LogType.Error
                );
            return null;
        }

        public Dictionary<string, StructureData> GetModelsData()
        {
            return modelsData;
        }

        private void GenerateDefaultFile()
        {
            logger.Log(
                $"Models data file not found at {config.ModelsDataFile}, generating default file.",
                Logging.LogType.Error
            );
            string modelsDir = config.ModelsDirectory;
            if (!Directory.Exists(modelsDir))
            {
                // TODO: consider connecting this to core service in case the users doesnt have the default models
                logger.Log(
                    $"Models directory not found at {modelsDir}, Please make sure to add yout models here!",
                    Logging.LogType.Error
                );
                Directory.CreateDirectory(modelsDir);
                return;
            }

            logger.Log($"Scanning models directory: {modelsDir}", Logging.LogType.Info);

            var modelFiles = Directory
                .GetFiles(modelsDir, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (modelFiles.Count == 0)
                logger.Log("No GLB models found.", Logging.LogType.Error);

            var models = modelFiles
                .Select(filePath =>
                {
                    string structureName = Path.GetFileNameWithoutExtension(filePath);
                    string fileName = Path.GetFileName(filePath);
                    var structureData = new StructureData(
                        id: Guid.NewGuid().ToString(),
                        structureName: structureName,
                        size: Vector3.one,
                        rotation: Vector3.zero,
                        fileName: fileName
                    );
                    return structureData;
                })
                .ToList();

            var serialized = new SerializedStructureData { structures = models };

            string json = JsonUtility.ToJson(serialized, true);
            File.WriteAllText(config.ModelsDataFile, json);

            logger.Log($"Generated models file with {models.Count} models", Logging.LogType.Info);
        }

        private List<StructureData> LoadFromDisk()
        {
            try
            {
                string json = File.ReadAllText(config.ModelsDataFile);
                var serialized = JsonUtility.FromJson<SerializedStructureData>(json);
                return serialized.structures;
            }
            catch (Exception ex)
            {
                logger.Log($"Error loading models data: {ex.Message}", Logging.LogType.Error);
                return new List<StructureData>();
            }
        }
    }
}
