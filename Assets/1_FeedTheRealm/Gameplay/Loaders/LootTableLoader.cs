using System.Collections.Generic;
using Models;
using UnityEngine;
using Utils;

[CreateAssetMenu(
    fileName = "LootTableLoader",
    menuName = "Scriptable Objects/Loaders/LootTableLoader"
)]
public class LootTableLoader : ScriptableObject, ILoadable, ICreatableLoader
{
    [SerializeField]
    private Logging.Logger logger;

    private List<CreatorObject> lootTables = new();

    void OnEnable()
    {
        SelectionRaiser.WorldSelected += LoadWorld;
    }

    void OnDisable()
    {
        SelectionRaiser.WorldSelected -= LoadWorld;
    }

    public List<CreatorObject> GetCreatables()
    {
        logger.Log($"Retrieving {lootTables.Count} loot tables", this, Logging.LogType.Info);
        return lootTables.FindAll(table => !table.IsDeleted);
    }

    public void AddCreatable(CreatorObject creatable)
    {
        lootTables.Add(creatable);
        logger.Log(
            $"Added new loot table: {creatable.DisplayName} (ID: {creatable.ObjectId})",
            this,
            Logging.LogType.Info
        );
    }

    public void RemoveCreatable(CreatorObject creatable)
    {
        creatable.Delete();
        lootTables.Remove(creatable);
    }

    public void UpdateCreatable(CreatorObject creatable)
    {
        int index = lootTables.FindIndex(table => table.ObjectId == creatable.ObjectId);
        if (index != -1)
        {
            lootTables[index] = creatable;
        }
    }

    public void LoadWorld(WorldData worldData)
    {
        lootTables.Clear();
        if (worldData == null)
        {
            logger.Log(
                "LootTableLoader.LoadWorld: worldData is null.",
                this,
                Logging.LogType.Warning
            );
            return;
        }

        foreach (LootTableData lootTableData in worldData.lootTables ?? new List<LootTableData>())
        {
            lootTables.Add(new LootTable(lootTableData));
        }
    }
}
