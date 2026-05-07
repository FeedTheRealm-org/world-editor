using System;
using System.Collections.Generic;
using System.IO;
using FeedTheRealm.Core.WorldObjects.Provider;
using FTR.Core.Common.Config;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Core.Repository
{
    public class ZoneMaterialsRepository : IInitializable
    {
        [Inject]
        private Config config;

        [Inject]
        private Logging.Logger logger;

        private Dictionary<string, Material> materialsCache;
        private HashSet<string> textureNames;

        public void Initialize() { }

        public ZoneMaterialsRepository(Config config, Logging.Logger logger)
        {
            this.config = config;
            this.logger = logger;
            Directory.CreateDirectory(config.ZoneMaterialsDirectory);
            textureNames = LoadTextureNamesFromDisk();
            textureNames.Add(config.defaultMaterialId);
            materialsCache = new Dictionary<string, Material>
            {
                [config.defaultMaterialId] = config.defaultZoneMaterial,
            };
            logger.Log(
                $"[ZoneMaterialsRepository] Initialized with {textureNames.Count} textures.",
                Logging.LogType.Info
            );
        }

        public List<string> GetMaterialNames() => new List<string>(textureNames);

        public Material GetMaterial(string name)
        {
            if (materialsCache.TryGetValue(name, out var cached))
                return cached;

            if (!textureNames.Contains(name))
            {
                logger.Log(
                    $"[ZoneMaterialsRepository] Texture '{name}' not found.",
                    Logging.LogType.Warning
                );
                return null;
            }

            var path = GetTexturePath(name);
            var material = CreateMaterialFromFile(path);
            if (material == null)
                return null;

            materialsCache[name] = material;
            return material;
        }

        public void AddMaterial(string sourcePath)
        {
            if (!File.Exists(sourcePath))
                return;

            string fileName = Path.GetFileName(sourcePath);
            string destPath = Path.Combine(config.ZoneMaterialsDirectory, fileName);
            File.Copy(sourcePath, destPath, overwrite: true);
            textureNames.Add(Path.GetFileNameWithoutExtension(fileName));
        }

        public void DeleteMaterial(string name)
        {
            var path = GetTexturePath(name);
            if (File.Exists(path))
                File.Delete(path);
            textureNames.Remove(name);
            materialsCache.Remove(name);
        }

        private HashSet<string> LoadTextureNamesFromDisk()
        {
            var result = new HashSet<string>();
            foreach (var ext in new[] { "*.png", "*.jpg", "*.jpeg" })
            foreach (var path in Directory.GetFiles(config.ZoneMaterialsDirectory, ext))
                result.Add(Path.GetFileNameWithoutExtension(path));
            return result;
        }

        private string GetTexturePath(string name)
        {
            foreach (var ext in new[] { ".png", ".jpg", ".jpeg" })
            {
                var path = Path.Combine(config.ZoneMaterialsDirectory, $"{name}{ext}");
                if (File.Exists(path))
                    return path;
            }
            return null;
        }

        private Material CreateMaterialFromFile(string path)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                var texture = new Texture2D(2, 2);
                if (!texture.LoadImage(bytes))
                {
                    logger.Log(
                        $"[ZoneMaterialsRepository] Failed to load texture from '{path}'.",
                        Logging.LogType.Error
                    );
                    return null;
                }

                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.mainTexture = texture;
                material.name = Path.GetFileNameWithoutExtension(path);
                return material;
            }
            catch (Exception ex)
            {
                logger.Log(
                    $"[ZoneMaterialsRepository] Error creating material from '{path}': {ex.Message}",
                    Logging.LogType.Error
                );
                return null;
            }
        }
    }
}
