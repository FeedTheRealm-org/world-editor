using FeedTheRealm.Core.EventChannels.WorldEvents;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Core.WorldObjects.Items
{
    public class WeaponItem : WorldObject
    {
        public WeaponItemData weaponItemData;

        public WeaponItem(CreatablesDataRegistryEvent registryEvent)
            : base(registryEvent) { }

        public override void SaveData(ref CreatablesData creatablesData)
        {
            creatablesData.weaponItems.Add(weaponItemData);
        }
    }
}
