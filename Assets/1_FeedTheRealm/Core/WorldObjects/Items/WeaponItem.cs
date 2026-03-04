using Models;

namespace FeedTheRealm.Core.WorldObjects.Items
{
    public class WeaponItem : Item
    {
        public WeaponType weaponType;
        public int damage;
        public float attackSpeed;
        public float range;
        public int ammo;

        public WeaponItem(WeaponItemData weaponItemData)
            : base(weaponItemData)
        {
            weaponType = weaponItemData.weaponType;
            damage = weaponItemData.damage;
            attackSpeed = weaponItemData.attackSpeed;
            range = weaponItemData.range;
            ammo = weaponItemData.ammo;
        }

        public override void SaveObject(ref WorldData worldData)
        {
            WeaponItemData weaponItemData = itemDataBuilder
                .SetItemData(ObjectId, DisplayName, description, spriteFile)
                .BuildWeaponItem(weaponType, damage, attackSpeed, range, ammo);
            worldData.weaponItems.Add(weaponItemData);
        }

        public override void DeleteObject(ref WorldData worldData)
        {
            worldData.weaponItems.RemoveAll(item => item.id == ObjectId);
        }
    }
}
