using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.Creatables
{
    public class FriendlyNpc : Creatable
    {
        public NPCData data { get; private set; }

        public FriendlyNpc(NPCData data)
        {
            this.data = data;
        }

        public override string Id => data.id;

        public override void Save(ref CreatablesData data)
        {
            data.npcs.Add(this.data);
        }
    }
}
