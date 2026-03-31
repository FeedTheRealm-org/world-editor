using System.IO;
using FeedTheRealm.Core.Utils;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.Creatables
{
    public class ConsumableItem : Creatable
    {
        public ConsumableItemData data { get; private set; }

        public ConsumableItem(ConsumableItemData data)
        {
            this.data = data;
        }

        public override string Id => data.id;

        public override void OnDelete()
        {
            if (data.spriteFilePath.StartsWith(config.SpritesDirectory))
                FileSystemHandler.DeleteFile(data.spriteFilePath);
        }

        public override void Save(ref CreatablesData creatablesData)
        {
            if (
                !string.IsNullOrEmpty(data.spriteFilePath) && Path.IsPathRooted(data.spriteFilePath)
            )
            {
                string savedFileName = FileSystemHandler.SaveSprite(
                    data.spriteFilePath,
                    config.SpritesDirectory,
                    data.id
                );

                if (savedFileName != null)
                    data.spriteFilePath = savedFileName;
            }

            creatablesData.consumableItems.Add(data);
        }
    }
}
