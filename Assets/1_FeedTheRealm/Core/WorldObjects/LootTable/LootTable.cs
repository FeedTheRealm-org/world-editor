using System.Collections.Generic;
using Models;
using LootEntryData = Models.LootTableData.LootEntryData;

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

    public override void DeleteObject(ref WorldData worldData)
    {
        worldData.lootTables.RemoveAll(lootTable => lootTable.id == ObjectId);
    }

    public override void SaveObject(ref WorldData worldData)
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
