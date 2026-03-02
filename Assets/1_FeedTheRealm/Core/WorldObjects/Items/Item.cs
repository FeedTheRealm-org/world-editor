using Builders;
using Models;

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
