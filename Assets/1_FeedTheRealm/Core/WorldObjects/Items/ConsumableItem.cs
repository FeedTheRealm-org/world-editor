using FTRShared.Runtime.Models;

namespace FeedTheRealm.Core.WorldObjects.Items
{
    public class ConsumableItem : Item
    {
        public EffectType effectType;
        public int value;
        public float duration;
        public float cooldown;
        public int maxStack;

        public ConsumableItem(ConsumableItemData consumableItemData)
            : base(consumableItemData)
        {
            effectType = consumableItemData.effectType;
            value = consumableItemData.value;
            duration = consumableItemData.duration;
            cooldown = consumableItemData.cooldown;
            maxStack = consumableItemData.maxStack;
        }

        public override void SaveObject(ref WorldData worldData)
        {
            ConsumableItemData consumableItemData = itemDataBuilder
                .SetItemData(ObjectId, DisplayName, description, spriteFile)
                .BuildConsumableItem(effectType, value, duration, cooldown, maxStack);
            worldData.consumableItems.Add(consumableItemData);
        }

        public override void DeleteObject(ref WorldData worldData)
        {
            worldData.consumableItems.RemoveAll(item => item.id == ObjectId);
        }
    }
}
