using System.Collections.Generic;
using Models;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ConsumableItemLoader",
    menuName = "Scriptable Objects/Loaders/Items/ConsumableItemLoader"
)]
public class ConsumableItemLoader : ItemLoader<ConsumableItemData>
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
