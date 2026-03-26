using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.Creatables
{
    public class LootTable : ICreatable
    {
        public LootTableData data { get; private set; }

        public LootTable(LootTableData data)
        {
            this.data = data;
        }

        public string Id => data.id;

        public void SaveData(ref CreatablesData data)
        {
            data.lootTables.Add(this.data);
        }
    }
}
