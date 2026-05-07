using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FTR.Core.Common.Config;
using UnityEngine;
using VContainer.Unity;

namespace FeedTheRealm.Core.Repository
{
    public enum ZoneTextureType
    {
        Ground,
        Skybox,
    }

    public class ZoneMaterialsRepository : IInitializable
    {
        private Config config;
        private Logging.Logger logger;

        private Dictionary<string, Material> groundMaterialsCache = new();
        private Dictionary<string, Material> skyboxMaterialsCache = new();
        private Dictionary<string, TextureEntry> groundTextures = new();
        private Dictionary<string, TextureEntry> skyboxTextures = new();

        public void Initialize() { }

        public const string defaultId = "Default";

        public ZoneMaterialsRepository(Config config, Logging.Logger logger)
        {
            this.config = config;
            this.logger = logger;
            Directory.CreateDirectory(config.ZoneGroundDirectory);
            Directory.CreateDirectory(config.ZoneSkyboxDirectory);

            groundTextures = LoadTexturesFromDisk(config.ZoneGroundDirectory);
            skyboxTextures = LoadTexturesFromDisk(config.ZoneSkyboxDirectory);

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

        public string DefaultMaterialId => defaultId;

        public bool IsDefaultMaterial(string name, ZoneTextureType type) =>
            name == (type == ZoneTextureType.Ground ? defaultId : defaultId);

        public Dictionary<string, Texture2D> GetTextures(ZoneTextureType type) =>
            GetTextureDict(type).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Texture);

        public void AddMaterial(string sourcePath, ZoneTextureType type)
        {
            if (!File.Exists(sourcePath))
                return;

            string directory =
                type == ZoneTextureType.Ground
                    ? config.ZoneGroundDirectory
                    : config.ZoneSkyboxDirectory;

            string originalName = Path.GetFileNameWithoutExtension(sourcePath).Replace("@", "-");
            if (originalName.Length > 100)
                originalName = originalName[..100];
            string id = Guid.NewGuid().ToString();
            string sanitizedName = $"{id}@{originalName}";
            string fileName = $"{sanitizedName}{Path.GetExtension(sourcePath)}";
            string destPath = Path.Combine(directory, fileName);

            File.Copy(sourcePath, destPath, overwrite: true);

            var texture = LoadTextureFromDisk(sanitizedName, destPath);
            if (texture != null)
                GetTextureDict(type)[sanitizedName] = new TextureEntry(destPath, texture);
        }

        public void DeleteMaterial(string name, ZoneTextureType type)
        {
            var dict = GetTextureDict(type);
            if (
                dict.TryGetValue(name, out var entry)
                && entry.Path != null
                && File.Exists(entry.Path)
            )
                File.Delete(entry.Path);
            dict.Remove(name);
            GetMaterialCache(type).Remove(name);
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

        // ------ PRIVATE METHODS ------

        private Dictionary<string, TextureEntry> GetTextureDict(ZoneTextureType type) =>
            type == ZoneTextureType.Ground ? groundTextures : skyboxTextures;

        private Dictionary<string, Material> GetMaterialCache(ZoneTextureType type) =>
            type == ZoneTextureType.Ground ? groundMaterialsCache : skyboxMaterialsCache;

        private void RegisterDefaultMaterials(
            List<Material> defaults,
            Dictionary<string, TextureEntry> textureDict,
            Dictionary<string, Material> materialCache
        )
        {
            foreach (var material in defaults)
            {
                if (material == null)
                    continue;
                textureDict[material.name] = new TextureEntry(
                    null,
                    material.mainTexture as Texture2D
                );
                materialCache[material.name] = material;
            }
        }

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
