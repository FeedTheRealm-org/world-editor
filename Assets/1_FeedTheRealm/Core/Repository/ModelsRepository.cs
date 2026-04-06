using System;
using System.Collections.Generic;
using System.IO;
using FTR.Core.Common.Config;
using FTRShared.Runtime.Models;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Core.Repository
{
    public class ModelsRepository : IInitializable
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
            Directory.CreateDirectory(config.ModelsDataDirectory);
            ScanForNewModels();
        }

        public void Initialize() { }

        public Dictionary<string, StructureData> GetModelsData()
        {
            modelsData ??= LoadAllFromDisk();
            return modelsData;
        }

        public string GetModelFilepath(string modelId)
        {
            GetModelsData().TryGetValue(modelId, out StructureData modelData);
            return Path.Combine(config.ModelsDirectory, modelData?.fileName ?? "");
        }

        /// <summary>
        /// Scans StreamingAssets/Models for any .glb files that don't yet
        /// have a corresponding metadata .json in persistentDataPath/Models.
        /// New models get a generated metadata file automatically.
        /// </summary>
        private void ScanForNewModels()
        {
            if (!Directory.Exists(config.ModelsDirectory))
            {
                logger.Log(
                    $"Models directory not found at '{config.ModelsDirectory}'. Add your .glb files there.",
                    Logging.LogType.Warning
                );
                Directory.CreateDirectory(config.ModelsDirectory);
                return;
            }

            var glbFiles = Directory.GetFiles(
                config.ModelsDirectory,
                "*.glb",
                SearchOption.AllDirectories
            );
            int generated = 0;

            foreach (var glbPath in glbFiles)
            {
                string modelName = Path.GetFileNameWithoutExtension(glbPath);
                string jsonPath = GetModelJsonPath(modelName);

                if (File.Exists(jsonPath))
                    continue;

                var model = new StructureData(
                    id: Guid.NewGuid().ToString(),
                    structureName: modelName,
                    size: Vector3.one,
                    rotation: Vector3.zero,
                    fileName: Path.GetFileName(glbPath)
                );

                WriteModelToDisk(model);
                generated++;
            }

            if (generated > 0)
                logger.Log(
                    $"[ModelsRepository] Generated metadata for {generated} new models.",
                    Logging.LogType.Info
                );
        }

        private Dictionary<string, StructureData> LoadAllFromDisk()
        {
            var result = new Dictionary<string, StructureData>();

            foreach (var jsonPath in Directory.GetFiles(config.ModelsDataDirectory, "*.json"))
            {
                try
                {
                    string json = File.ReadAllText(jsonPath);
                    var model = JsonUtility.FromJson<StructureData>(json);
                    if (model != null && !string.IsNullOrEmpty(model.id))
                        result[model.id] = model;
                }
                catch (Exception ex)
                {
                    logger.Log(
                        $"[ModelsRepository] Error loading '{jsonPath}': {ex.Message}",
                        Logging.LogType.Error
                    );
                }
            }

            logger.Log($"[ModelsRepository] Loaded {result.Count} models.", Logging.LogType.Info);
            return result;
        }

        public void WriteModelToDisk(StructureData model)
        {
            try
            {
                string json = JsonUtility.ToJson(model, true);
                File.WriteAllText(GetModelJsonPath(model.structureName), json);
            }
            catch (Exception ex)
            {
                logger.Log(
                    $"[ModelsRepository] Failed to write '{model.structureName}': {ex.Message}",
                    Logging.LogType.Error
                );
            }
        }

        public void DeleteModelFromDisk(string modelName)
        {
            string jsonPath = GetModelJsonPath(modelName);
            if (!File.Exists(jsonPath))
                return;
            File.Delete(jsonPath);
            modelsData = null; // invalidate cache
            logger.Log(
                $"[ModelsRepository] Deleted model metadata for '{modelName}'.",
                Logging.LogType.Info
            );
        }

        /// <summary>
        /// Adds a new model to the repository and writes its metadata to disk.
        /// Call this after copying the .glb file to the models directory.
        /// </summary>
        public void AddModel(StructureData model, string sourceFilePath)
        {
            if (!File.Exists(sourceFilePath))
            {
                logger.Log(
                    $"[ModelsRepository] Source file not found: {sourceFilePath}",
                    Logging.LogType.Error
                );
                return;
            }

            string destPath = Path.Combine(config.ModelsDirectory, model.fileName);
            File.Copy(sourceFilePath, destPath, overwrite: true);
            logger.Log(
                $"[ModelsRepository] Copied model file to '{destPath}'.",
                Logging.LogType.Info
            );

            WriteModelToDisk(model);
            modelsData ??= new Dictionary<string, StructureData>();
            modelsData[model.id] = model;
            logger.Log(
                $"[ModelsRepository] Added model '{model.structureName}' (id: {model.id}).",
                Logging.LogType.Info
            );
        }

        private string GetModelJsonPath(string modelName) =>
            Path.Combine(config.ModelsDataDirectory, $"{modelName}.json");
    }
}
