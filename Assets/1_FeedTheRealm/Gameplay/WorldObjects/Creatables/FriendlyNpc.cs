using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.Creatables
{
    public class FriendlyNpc : ICreatable
    {
        public NPCData data { get; private set; }

        public FriendlyNpc(NPCData data)
        {
            this.data = data;
        }

        public string Id => data.id;

        public void SaveData(ref CreatablesData data)
        {
            data.npcs.Add(this.data);
        }
    }
}
