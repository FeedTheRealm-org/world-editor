using FeedTheRealm.Core.Utils;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.Creatables
{
    public class Weapon : Creatable
    {
        public WeaponItemData data { get; private set; }

        public Weapon(WeaponItemData data)
        {
            this.data = data;
        }

        public override void OnDelete()
        {
            if (data.spriteFilePath.StartsWith(config.SpritesDirectory))
                FileSystemHandler.DeleteFile(data.spriteFilePath);
        }

        public override string Id => data.id;

        public override void Save(ref CreatablesData data)
        {
            data.weaponItems.Add(this.data);
        }
    }
}
