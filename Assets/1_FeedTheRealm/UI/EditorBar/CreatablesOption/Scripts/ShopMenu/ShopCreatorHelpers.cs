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

            string path = entry.sprite_path;
            string fullPath = ResolveFullPath(path);
            var fullSprite = CustomFileBrowser.LoadSpriteFromDisk(fullPath);

            if (fullSprite != null && fullSprite.texture != null)
            {
                Sprite cropped = CosmeticIconLoader.CreateCroppedSprite(
                    fullSprite.texture,
                    product.categoryName
                );
                if (cropped != null)
                {
                    image.sprite = cropped;
                    image.scaleMode = ScaleMode.ScaleToFit;
                    return;
                }
            }

            LoadSprite(path, image);
        }

        private static string ResolveFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (Path.IsPathRooted(path))
                return path;

            return Path.Combine(SpritesBasePath, path);
        }
    }
}
