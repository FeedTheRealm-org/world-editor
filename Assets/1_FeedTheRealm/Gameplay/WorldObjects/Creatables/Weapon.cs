using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.Creatables
{
    public class Weapon : ICreatable
    {
        public WeaponItemData data { get; private set; }

        public Weapon(WeaponItemData data)
        {
            this.data = data;
        }

        public string Id => data.id;

        public void SaveData(ref CreatablesData data)
        {
            data.weaponItems.Add(this.data);
        }
    }
}
