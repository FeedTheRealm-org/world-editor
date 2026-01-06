using Models;

public class ConsumableItem : CreatorObject
{
    public string description;
    public EffectType effectType;
    public int value;
    public float duration;
    public float cooldown;
    public int maxStack;

    public ConsumableItem(ConsumableItemData consumableItemData)
        : base(consumableItemData.name, consumableItemData.id, consumableItemData.spriteId)
    {
        description = consumableItemData.description;
        effectType = consumableItemData.effectType;
        value = consumableItemData.value;
        duration = consumableItemData.duration;
        cooldown = consumableItemData.cooldown;
        maxStack = consumableItemData.maxStack;
    }

    public override void SaveObject(ref WorldData worldData)
    {
        ConsumableItemData itemData = new(
            ObjectId,
            DisplayName,
            description,
            effectType,
            value,
            duration,
            cooldown,
            maxStack,
            spriteFile
        );
        worldData.consumableItems.Add(itemData);
    }

    public override void DeleteObject(ref WorldData worldData)
    {
        worldData.consumableItems.RemoveAll(item => item.id == ObjectId);
    }
}
