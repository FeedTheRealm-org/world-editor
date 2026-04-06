using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.Creatables
{
    public class Portal : Creatable
    {
        public PortalData data { get; private set; }

        public Portal(PortalData data)
        {
            this.data = data;
        }

        public Portal(int zoneId)
        {
            string id = System.Guid.NewGuid().ToString();
            data = new PortalData(id: id, name: $"Portal-{id[..5]}", zoneId: zoneId);
        }

        public override string Id => data.id;

        public override void Save(ref CreatablesData data)
        {
            data.portals.Add(this.data);
        }
    }
}
