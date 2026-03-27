using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.Creatables
{
    public class AggresiveNpc : Creatable
    {
        public EnemyData data { get; private set; }

        public AggresiveNpc(EnemyData data)
        {
            this.data = data;
        }

        public override string Id => data.id;

        public override void Save(ref CreatablesData data)
        {
            data.enemies.Add(this.data);
        }
    }
}
