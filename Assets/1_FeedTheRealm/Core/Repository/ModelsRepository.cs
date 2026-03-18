using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FTR.Core.Common.Config;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Core.Repository
{
    [CreateAssetMenu(fileName = "ModelsRepository", menuName = "Repository/ModelsRepository")]
    public class ModelsRepository : ScriptableObject
    {
        [Inject]
        private Config config;

        [Inject]
        private Logging.Logger logger;
        private bool isInitialized = false;
        private Dictionary<string, ModelData> modelsData;

        public void Initialize()
        {
            if (config.ForceReinitialize)
                isInitialized = false;

            if (isInitialized)
                return;

            if (!File.Exists(config.ModelsDataFile))
                GenerateDefaultFile();

            var modelsList = LoadFromDisk();
            modelsData = modelsList.ToDictionary(m => m.name, m => m);
            logger.Log($"ModelsRepository loaded {modelsData.Count} models", Logging.LogType.Info);
            isInitialized = true;
        }

        public ModelData GetModelData(string modelName)
        {
            if (modelsData.TryGetValue(modelName, out var modelData))
                return modelData;
            else
                logger.Log($"Model {modelName} not found in repository", Logging.LogType.Error);
            return null;
        }

        public List<string> ListAvailableModels()
        {
            return modelsData.Keys.ToList();
        }

        private void GenerateDefaultFile()
        {
            logger.Log(
                $"Models data file not found at {config.ModelsDataFile}, generating default file.",
                Logging.LogType.Warning
            );
            string modelsDir = config.ModelsDirectory;
            if (!Directory.Exists(modelsDir))
            {
                // TODO: consider connecting this to core service in case the users doesnt have the default models
                logger.Log(
                    $"Models directory not found at {modelsDir}, Please make sure to add yout models here!",
                    Logging.LogType.Warning
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
                logger.Log("No GLB models found.", Logging.LogType.Warning);

            var models = modelFiles
                .Select(filePath =>
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    return new ModelData
                    {
                        id = Guid.NewGuid().ToString(),
                        name = fileName,
                        filePath = filePath,
                        defaultRotation = Vector3.zero,
                        defaultScale = Vector3.one,
                        colliders = new List<BoxColliderData>(),
                    };
                })
                .ToList();

            var serialized = new SerializedModelData { models = models };

            string json = JsonUtility.ToJson(serialized, true);
            File.WriteAllText(config.ModelsDataFile, json);

            logger.Log($"Generated models file with {models.Count} models", Logging.LogType.Info);
        }

        private List<ModelData> LoadFromDisk()
        {
            try
            {
                string json = File.ReadAllText(config.ModelsDataFile);
                var serialized = JsonUtility.FromJson<SerializedModelData>(json);
                return serialized.models;
            }
            catch (Exception ex)
            {
                logger.Log($"Error loading models data: {ex.Message}", Logging.LogType.Error);
                return new List<ModelData>();
            }
        }
    }
}
