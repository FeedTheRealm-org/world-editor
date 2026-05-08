using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FTR.Core.Common.Config;
using UnityEngine;
using VContainer.Unity;

namespace FeedTheRealm.Core.Repository
{
    public class ZoneMaterialsRepository : IInitializable
    {
        private Config config;
        private Logging.Logger logger;

        private Dictionary<string, Material> groundMaterialsCache = new();
        private Dictionary<string, Material> skyboxMaterialsCache = new();
        private Dictionary<string, TextureEntry> groundTextures = new();
        private Dictionary<string, TextureEntry> skyboxTextures = new();

        private const string SEPARATOR = "@";

        public void Initialize() { }

        public const string defaultId = "Default";
        public string DefaultMaterialId => defaultId;

        public ZoneMaterialsRepository(Config config, Logging.Logger logger)
        {
            this.config = config;
            this.logger = logger;
            Directory.CreateDirectory(config.MaterialsDirectory);

            groundTextures = LoadTexturesFromDisk(config.MaterialsDirectory);
            skyboxTextures = LoadTexturesFromDisk(config.MaterialsDirectory);

            // Register single defaults with "Default" key
            groundTextures[defaultId] = new TextureEntry(
                null,
                config.defaultGroundMaterial?.mainTexture as Texture2D
            );
            groundMaterialsCache[defaultId] = config.defaultGroundMaterial;

            skyboxTextures[defaultId] = new TextureEntry(null, null);
            skyboxMaterialsCache[defaultId] = config.defaultSkyboxMaterial;

            logger.Log(
                $"[ZoneMaterialsRepository] Initialized with {groundTextures.Count} ground and {skyboxTextures.Count} skybox textures.",
                Logging.LogType.Info
            );
        }

        public Material GetDefaultMaterial(ZoneTextureType type)
        {
            var id = type == ZoneTextureType.Ground ? defaultId : defaultId;
            return GetMaterial(id, type);
        }

        public string GetMaterialFilePath(string name, ZoneTextureType type)
        {
            var dict = GetTextureDict(type);
            return dict.TryGetValue(name, out var entry) ? entry.Path : null;
        }

        public bool IsDefaultMaterial(string name, ZoneTextureType type) =>
            name == (type == ZoneTextureType.Ground ? defaultId : defaultId);

        public Dictionary<string, Texture2D> GetTextures(ZoneTextureType type) =>
            GetTextureDict(type).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Texture);

        public void AddMaterial(string sourcePath) =>
            AddMaterialInternal(sourcePath, Guid.NewGuid().ToString());

        public void AddDefaultMaterial(string sourcePath) =>
            AddMaterialInternal(sourcePath, defaultId);

        private void AddMaterialInternal(string sourcePath, string id)
        {
            if (!File.Exists(sourcePath))
                return;

            string originalName = Path.GetFileNameWithoutExtension(sourcePath)
                .Replace(SEPARATOR, "-");
            if (originalName.Length > 100)
                originalName = originalName[..100];

            string sanitizedName = $"{id}{SEPARATOR}{originalName}";
            string fileName = $"{sanitizedName}{Path.GetExtension(sourcePath)}";
            string destPath = Path.Combine(config.MaterialsDirectory, fileName);

            File.Copy(sourcePath, destPath, overwrite: true);

            var texture = LoadTextureFromDisk(sanitizedName, destPath);
            if (texture != null)
            {
                var entry = new TextureEntry(destPath, texture);
                groundTextures[sanitizedName] = entry;
                skyboxTextures[sanitizedName] = entry;
            }
        }

        public void DeleteMaterial(string name)
        {
            // Delete the file once (check either dict since they share the same path)
            if (
                groundTextures.TryGetValue(name, out var entry)
                && entry.Path != null
                && File.Exists(entry.Path)
            )
                File.Delete(entry.Path);

            groundTextures.Remove(name);
            skyboxTextures.Remove(name);
            groundMaterialsCache.Remove(name);
            skyboxMaterialsCache.Remove(name);
        }

        public Material GetMaterial(string name, ZoneTextureType type)
        {
            var cache = GetMaterialCache(type);
            if (cache.TryGetValue(name, out var cached))
                return cached;

            var textureDict = GetTextureDict(type);
            if (!textureDict.TryGetValue(name, out var entry) || entry.Texture == null)
                return null;

            Material material;
            if (type == ZoneTextureType.Skybox)
            {
                material = new Material(Shader.Find("Skybox/Panoramic"));
                material.SetTexture("_MainTex", entry.Texture);
            }
            else
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.mainTexture = entry.Texture;
            }

            material.name = name;
            cache[name] = material;
            return material;
        }

        public string GetPublishId(string name, ZoneTextureType type)
        {
            var dict = GetTextureDict(type);
            if (dict.TryGetValue(name, out var _))
            {
                string[] parts = name.Split(SEPARATOR);
                if (parts.Length > 1 && parts[0] != defaultId)
                    return parts[0];
            }
            return null;
        }

        // ------ PRIVATE METHODS ------

        private Dictionary<string, TextureEntry> GetTextureDict(ZoneTextureType type) =>
            type == ZoneTextureType.Ground ? groundTextures : skyboxTextures;

        private Dictionary<string, Material> GetMaterialCache(ZoneTextureType type) =>
            type == ZoneTextureType.Ground ? groundMaterialsCache : skyboxMaterialsCache;

        private Dictionary<string, TextureEntry> LoadTexturesFromDisk(string directory)
        {
            var result = new Dictionary<string, TextureEntry>();
            foreach (var ext in new[] { "*.png", "*.jpg", "*.jpeg" })
            foreach (var path in Directory.GetFiles(directory, ext))
            {
                string name = Path.GetFileNameWithoutExtension(path);
                var texture = LoadTextureFromDisk(name, path);
                if (texture != null)
                    result[name] = new TextureEntry(path, texture);
            }
            return result;
        }

        private Texture2D LoadTextureFromDisk(string name, string path)
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
                texture.name = name;
                return texture;
            }
            catch (Exception ex)
            {
                logger.Log(
                    $"[ZoneMaterialsRepository] Error loading texture '{name}': {ex.Message}",
                    Logging.LogType.Error
                );
                return null;
            }
        }

        private struct TextureEntry
        {
            public string Path;
            public Texture2D Texture;

            public TextureEntry(string path, Texture2D texture)
            {
                Path = path;
                Texture = texture;
            }
        }
    }
}
