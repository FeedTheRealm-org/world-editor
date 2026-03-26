using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.Creatables
{
    public class AggresiveNpc : ICreatable
    {
        public EnemyData data { get; private set; }

        public AggresiveNpc(EnemyData data)
        {
            this.data = data;
        }

        public string Id => data.id;

        public void SaveData(ref CreatablesData data)
        {
            data.enemies.Add(this.data);
        }
    }
}
