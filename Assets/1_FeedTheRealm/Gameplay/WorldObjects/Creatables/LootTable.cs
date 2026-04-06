using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.Creatables
{
    public class LootTable : Creatable
    {
        public LootTableData data { get; private set; }

        public LootTable(LootTableData data)
        {
            this.data = data;
        }

        public override string Id => data.id;

        public override void Save(ref CreatablesData data)
        {
            data.lootTables.Add(this.data);
        }
    }
}
