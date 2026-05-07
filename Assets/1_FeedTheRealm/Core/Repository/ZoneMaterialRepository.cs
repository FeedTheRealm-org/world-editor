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

        public ZoneMaterialsRepository(Config config, Logging.Logger logger)
        {
            this.config = config;
            this.logger = logger;
            Directory.CreateDirectory(config.ZoneGroundDirectory);
            Directory.CreateDirectory(config.ZoneSkyboxDirectory);

            groundTextures = LoadTexturesFromDisk(config.ZoneGroundDirectory);
            groundTextures[config.defaultMaterialId] = new TextureEntry(
                null,
                config.defaultZoneMaterial.mainTexture as Texture2D
            );
            groundMaterialsCache[config.defaultMaterialId] = config.defaultZoneMaterial;

            skyboxTextures = LoadTexturesFromDisk(config.ZoneSkyboxDirectory);

            logger.Log(
                $"[ZoneMaterialsRepository] Initialized with {groundTextures.Count} ground and {skyboxTextures.Count} skybox textures.",
                Logging.LogType.Info
            );
        }

        public string DefaultMaterialId => config.defaultMaterialId;

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
            string fileName = Path.GetFileName(sourcePath);
            string destPath = Path.Combine(directory, fileName);
            File.Copy(sourcePath, destPath, overwrite: true);

            string name = Path.GetFileNameWithoutExtension(fileName);
            var texture = LoadTextureFromDisk(name, destPath);
            if (texture != null)
                GetTextureDict(type)[name] = new TextureEntry(destPath, texture);
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

            var shader =
                type == ZoneTextureType.Ground
                    ? Shader.Find("Universal Render Pipeline/Lit")
                    : Shader.Find("Skybox/Panoramic");

            var material = new Material(shader);
            material.mainTexture = entry.Texture;
            material.name = name;
            cache[name] = material;
            return material;
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
