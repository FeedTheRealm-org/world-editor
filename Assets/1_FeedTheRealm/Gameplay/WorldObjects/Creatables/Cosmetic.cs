using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.Creatables
{
    public class Cosmetic : Creatable
    {
        public CosmeticData data { get; private set; }

        public Cosmetic(CosmeticData data)
        {
            this.data = data;
        }

        public override string Id => data.id;

        public override void Save(ref CreatablesData data)
        {
            data.cosmetics.Add(this.data);
        }
    }
}
