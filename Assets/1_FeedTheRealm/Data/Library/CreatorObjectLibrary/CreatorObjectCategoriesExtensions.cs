using System;
using System.Collections.Generic;

public static class CreatorObjectCategoriesExtensions
{
    private static readonly Dictionary<CreatorObjectCategories, string> CategoryNames = new()
    {
        { CreatorObjectCategories.ConsumableItem, "Consumable Item" },
        { CreatorObjectCategories.WeaponItem, "Weapon Item" },
        { CreatorObjectCategories.Enemy, "Enemy" },
        { CreatorObjectCategories.LootTable, "Loot Table" },
    };

    public static string GetDisplayName(this CreatorObjectCategories category)
    {
        return CategoryNames.TryGetValue(category, out var name) ? name : category.ToString();
    }

    public static bool TryGetCategory(string name, out CreatorObjectCategories category)
    {
        foreach (var kvp in CategoryNames)
        {
            if (kvp.Value == name)
            {
                category = kvp.Key;
                return true;
            }
        }
        category = default;
        return false;
    }
}
