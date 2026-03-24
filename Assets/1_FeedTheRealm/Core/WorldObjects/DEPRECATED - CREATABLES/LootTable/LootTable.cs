using System.Collections.Generic;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FTRShared.Runtime.Models;
using LootEntryData = FTRShared.Runtime.Models.LootTableData.LootEntryData;

namespace FeedTheRealm.Core.WorldObjects.LootTable
{
    public class LootTable : CreatorObject
    {
        public int minGoldDropAmount;
        public int maxGoldDropAmount;
        public List<LootEntryData> lootItems = new();

        public LootTable(LootTableData lootTableData)
            : base(lootTableData.name, lootTableData.id)
        {
            minGoldDropAmount = lootTableData.minGoldDropAmount;
            maxGoldDropAmount = lootTableData.maxGoldDropAmount;
            lootItems = lootTableData.lootItems;
        }

        public override void DeleteObject(ref WorldDataOld worldData)
        {
            worldData.lootTables.RemoveAll(lootTable => lootTable.id == ObjectId);
        }

        public override void SaveObject(ref WorldDataOld worldData)
        {
            LootTableData lootTableData = new(
                ObjectId,
                DisplayName,
                minGoldDropAmount,
                maxGoldDropAmount,
                lootItems
            );
            worldData.lootTables.Add(lootTableData);
        }
    }
}
