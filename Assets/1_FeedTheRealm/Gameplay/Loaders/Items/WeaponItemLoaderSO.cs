using System.Collections.Generic;
using Models;
using UnityEngine;

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
