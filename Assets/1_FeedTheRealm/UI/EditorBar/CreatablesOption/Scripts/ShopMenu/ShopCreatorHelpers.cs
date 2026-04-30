using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FeedTheRealm.Gameplay.Creatables;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace FeedTheRealm.UI.EditorBar.CreatablesOption.Scripts.ShopMenu
{
    internal class GoldItemBinding
    {
        public ProductData product;
    }

    internal class CosmeticItemBinding
    {
        public ProductData product;
        public Cosmetic cosmetic;
        public Action onDelete;
        public Action onEdit;
        public Action onConfirm;
    }

    internal static class ShopSpriteLoader
    {
        private static SpriteConfigBuilder _configBuilder;
        private static SpriteConfigDirector _configDirector;
        public static string SpritesBasePath { get; set; } = Application.streamingAssetsPath;

        internal static void LoadSprite(string path, Image image)
        {
            if (image == null || string.IsNullOrEmpty(path))
                return;

            string fullPath = ResolveFullPath(path);
            var sprite = CustomFileBrowser.LoadSpriteFromDisk(fullPath);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.scaleMode = ScaleMode.ScaleToFit;
            }
        }

        internal static void LoadItemSprite(object item, Image image)
        {
            string path = item switch
            {
                ConsumableItem c => c.data.spriteFilePath,
                Weapon w => w.data.spriteFilePath,
                Cosmetic c => c
                    .data.categories.Values.Select(e => e.sprite_path)
                    .FirstOrDefault(v => !string.IsNullOrEmpty(v)),
                _ => null,
            };
            LoadSprite(path, image);
        }

        internal static void LoadCosmeticSprite(Cosmetic cosmetic, ProductData product, Image image)
        {
            if (cosmetic == null || product == null || image == null)
                return;

            if (
                !cosmetic.data.categories.TryGetValue(product.categoryName, out var entry)
                || string.IsNullOrEmpty(entry.sprite_path)
            )
                return;

            CharacterPartCategory part = CosmeticPartResolver.Resolve(product.categoryName);

            if (part != CharacterPartCategory.None)
            {
                TryLoadCroppedSprite(entry.sprite_path, part, image);
                return;
            }

            LoadSprite(entry.sprite_path, image);
        }

        private static void TryLoadCroppedSprite(
            string path,
            CharacterPartCategory part,
            Image image
        )
        {
            EnsureDirectorInitialized();

            string fullPath = ResolveFullPath(path);

            var fullSprite = CustomFileBrowser.LoadSpriteFromDisk(fullPath);

            if (fullSprite == null || fullSprite.texture == null)
            {
                LoadSprite(path, image);
                return;
            }

            var configs = GetSpriteConfigsForPart(part);
            var frontConfig = configs.FirstOrDefault(c => c.Direction == FacingDirection.Front);

            if (frontConfig == null)
            {
                Debug.LogWarning(
                    $"[ShopSpriteLoader] Couldn't find Front config for '{part}', using complete sprite."
                );
                LoadSprite(path, image);
                return;
            }

            Texture2D texture = fullSprite.texture;
            Rect rect = frontConfig.Rect;

            if (
                rect.x + rect.width > texture.width
                || rect.y + rect.height > texture.height
                || rect.width <= 0
                || rect.height <= 0
            )
            {
                LoadSprite(path, image);
                return;
            }

            Sprite cropped = Sprite.Create(
                texture,
                rect,
                frontConfig.Pivot,
                frontConfig.PixelsPerUnit
            );

            image.sprite = cropped;
            image.scaleMode = ScaleMode.ScaleToFit;
        }

        private static List<SpriteConfig> GetSpriteConfigsForPart(CharacterPartCategory part)
        {
            return part switch
            {
                CharacterPartCategory.ArmorHelmet => _configDirector.BuildArmorHelmetSpriteConfig(),
                CharacterPartCategory.ArmorBody => _configDirector.BuildArmorBodySpriteConfig(),
                CharacterPartCategory.ArmorArmR => _configDirector.BuildArmorArmsSpriteConfig(),
                CharacterPartCategory.ArmorArmL => _configDirector.BuildArmorArmsSpriteConfig(),
                CharacterPartCategory.ArmorSleeveR =>
                    _configDirector.BuildArmorSleevesSpriteConfig(),
                CharacterPartCategory.ArmorSleeveL =>
                    _configDirector.BuildArmorSleevesSpriteConfig(),
                CharacterPartCategory.ArmorHandR => _configDirector.BuildArmorHandsSpriteConfig(),
                CharacterPartCategory.ArmorHandL => _configDirector.BuildArmorHandsSpriteConfig(),
                CharacterPartCategory.ArmorLegR => _configDirector.BuildArmorLegsSpriteConfig(),
                CharacterPartCategory.ArmorLegL => _configDirector.BuildArmorLegsSpriteConfig(),
                CharacterPartCategory.Hair => _configDirector.BuildHairSpriteConfig(),
                CharacterPartCategory.Beard => _configDirector.BuildBeardSpriteConfig(),
                CharacterPartCategory.EyeBrows => _configDirector.BuildEyeBrowsSpriteConfig(),
                CharacterPartCategory.Eyes => _configDirector.BuildEyesSpriteConfig(),
                CharacterPartCategory.Mouth => _configDirector.BuildMouthSpriteConfig(),
                CharacterPartCategory.EarringR => _configDirector.BuildEarringsSpriteConfig(),
                CharacterPartCategory.EarringL => _configDirector.BuildEarringsSpriteConfig(),
                CharacterPartCategory.Back => _configDirector.BuildBackSpriteConfig(),
                CharacterPartCategory.Mask => _configDirector.BuildMaskSpriteConfig(),
                CharacterPartCategory.EquipmentR => _configDirector.BuildEquipmentSpriteConfig(),
                _ => new List<SpriteConfig>(),
            };
        }

        private static string ResolveFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (Path.IsPathRooted(path))
                return path;

            return Path.Combine(SpritesBasePath, path);
        }

        private static void EnsureDirectorInitialized()
        {
            if (_configBuilder == null)
            {
                _configBuilder = new SpriteConfigBuilder();
                _configDirector = new SpriteConfigDirector(_configBuilder);
            }
        }
    }
}
