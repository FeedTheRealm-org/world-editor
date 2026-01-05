using Models;

public class ConsumableItem : CreatorObject
{
    public string description;
    public EffectType effectType;
    public int value;
    public float duration;
    public float cooldown;
    public int maxStack;
    public string spriteId;

    public ConsumableItem(ConsumableItemData consumableItemData)
        : base(consumableItemData.name, consumableItemData.id)
    {
        description = consumableItemData.description;
        effectType = consumableItemData.effectType;
        value = consumableItemData.value;
        duration = consumableItemData.duration;
        cooldown = consumableItemData.cooldown;
        maxStack = consumableItemData.maxStack;
        spriteId = consumableItemData.spriteId;
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
            spriteId
        );
        worldData.consumableItems.Add(itemData);
    }

    public override void DeleteObject(ref WorldData worldData)
    {
        worldData.consumableItems.RemoveAll(item => item.id == ObjectId);
    }
}
