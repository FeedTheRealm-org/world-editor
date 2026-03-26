using System;
using System.Collections.Generic;

namespace FeedTheRealm.Gameplay.Library.CreatorObjectLibrary
{
    public static class CreatableObjectCategoriesExtensions
    {
        //     private static readonly Dictionary<CreatableObjectCategories, string> CategoryNames = new()
        //     {
        //         { CreatableObjectCategories.ConsumableItem, "Consumable Item" },
        //         { CreatableObjectCategories.WeaponItem, "Weapon Item" },
        //         { CreatableObjectCategories.Enemy, "Enemy" },
        //         { CreatableObjectCategories.LootTable, "Loot Table" },
        //         { CreatableObjectCategories.Dialog, "Dialog" },
        //         { CreatableObjectCategories.Message, "Message" },
        //         { CreatableObjectCategories.NPC, "NPC" },
        //         { CreatableObjectCategories.Quest, "Quest" },
        //     };

        //     public static string GetDisplayName(this CreatableObjectCategories category)
        //     {
        //         return CategoryNames.TryGetValue(category, out var name) ? name : category.ToString();
        //     }

        //     public static bool TryGetCategory(string name, out CreatableObjectCategories category)
        //     {
        //         foreach (var kvp in CategoryNames)
        //         {
        //             if (kvp.Value == name)
        //             {
        //                 category = kvp.Key;
        //                 return true;
        //             }
        //         }
        //         category = default;
        //         return false;
        //     }
    }
}
