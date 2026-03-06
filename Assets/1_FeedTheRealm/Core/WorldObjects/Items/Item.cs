using Builders;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Core.WorldObjects.Items
{
    public abstract class Item : CreatorObject
    {
        public string description;
        protected ItemDataBuilder itemDataBuilder = new ItemDataBuilder();

        public Item(ItemData itemData)
            : base(itemData.name, itemData.id, itemData.spriteFilePath)
        {
            description = itemData.description;
        }
    }
}
