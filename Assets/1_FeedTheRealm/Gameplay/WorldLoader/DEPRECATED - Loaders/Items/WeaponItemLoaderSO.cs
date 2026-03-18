using System.Collections.Generic;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FeedTheRealm.Core.WorldObjects.Items;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldLoader.Items
{
    [CreateAssetMenu(
        fileName = "WeaponItemLoader",
        menuName = "Scriptable Objects/Loaders/Items/WeaponItemLoader"
    )]
    public class WeaponItemLoaderSO : ItemLoader<WeaponItemData>
    {
        protected override IEnumerable<WeaponItemData> GetData(WorldData worldData)
        {
            return worldData.weaponItems ?? new List<WeaponItemData>();
        }

        protected override CreatorObject CreateItem(WeaponItemData data)
        {
            return new WeaponItem(data);
        }
    }
}
