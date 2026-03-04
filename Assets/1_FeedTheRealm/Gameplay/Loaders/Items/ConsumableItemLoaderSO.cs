using System.Collections.Generic;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FeedTheRealm.Core.WorldObjects.Items;
using Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.Loaders.Items
{
    [CreateAssetMenu(
        fileName = "ConsumableItemLoader",
        menuName = "Scriptable Objects/Loaders/Items/ConsumableItemLoader"
    )]
    public class ConsumableItemLoaderSO : ItemLoader<ConsumableItemData>
    {
        protected override IEnumerable<ConsumableItemData> GetData(WorldData worldData)
        {
            return worldData.consumableItems ?? new List<ConsumableItemData>();
        }

        protected override CreatorObject CreateItem(ConsumableItemData data)
        {
            return new ConsumableItem(data);
        }
    }
}
