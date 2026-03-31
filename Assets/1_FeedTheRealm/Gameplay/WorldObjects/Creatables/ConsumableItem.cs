using System.IO;
using FeedTheRealm.Core.Utils;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;

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
            if (!data.spriteFilePath.StartsWith(config.SpritesDirectory))
            {
                data.spriteFilePath = FileSystemHandler.SaveSprite(
                    data.spriteFilePath,
                    config.SpritesDirectory,
                    data.id
                );
            }
            creatablesData.consumableItems.Add(data);
        }
    }
}
