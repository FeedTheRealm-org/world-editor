using System;
using System.Linq;
using FeedTheRealm.Gameplay.Creatables;
using FTRShared.Runtime.Models;
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
        internal static void LoadSprite(string path, Image image)
        {
            if (image == null || string.IsNullOrEmpty(path))
                return;
            var sprite = CustomFileBrowser.LoadSpriteFromDisk(path);
            if (sprite != null)
                image.sprite = sprite;
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

        internal static void LoadCosmeticSprite(Cosmetic cosmetic, string categoryName, Image image)
        {
            if (
                !cosmetic.data.categories.TryGetValue(categoryName, out var entry)
                || string.IsNullOrEmpty(entry.sprite_path)
            )
                return;
            LoadSprite(entry.sprite_path, image);
        }
    }
}
