using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.Creatables
{
    public class ConsumableItem : ICreatable
    {
        public ConsumableItemData data { get; private set; }

        public ConsumableItem(ConsumableItemData data)
        {
            this.data = data;
        }

        public string Id => data.id;

        public void SaveData(ref CreatablesData data)
        {
            data.consumableItems.Add(this.data);
        }
    }
}
