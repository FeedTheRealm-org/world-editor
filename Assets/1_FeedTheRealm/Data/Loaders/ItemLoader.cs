using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using UnityEngine;
using Utils;

[CreateAssetMenu(
    fileName = "ItemLoader",
    menuName = "Scriptable Objects/WorldEditor/ItemLoader"
)]
public class ItemLoader : ScriptableObject, ILoadable
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private ConsumableItems consumableItemsDatabase;

    void OnEnable()
    {
        SelectionRaiser.WorldSelected += LoadWorld;
    }

    void OnDisable()
    {
        SelectionRaiser.WorldSelected -= LoadWorld;
    }

    // For items we currently don't have a separate shared library to load.
    // This method is kept to satisfy the ILoadable contract.
    public void LoadLibrary()
    {
    }

    // When a world is selected, populate the ConsumableItems database
    // from the world data so the editor UIs work with the correct items.
    public void LoadWorld(WorldData worldData)
    {
        if (worldData == null)
        {
            logger.Log("ItemLoader.LoadWorld: worldData is null.", this, Logging.LogType.Warning);
            return;
        }

        if (consumableItemsDatabase == null)
        {
            logger.Log(
                "ItemLoader.LoadWorld: consumableItemsDatabase is not assigned.",
                this,
                Logging.LogType.Error
            );
            return;
        }

        var itemsFromWorld = worldData.consumableItems ?? new List<ConsumableItem>();
        logger.Log(
            $"ItemLoader.LoadWorld: loading {itemsFromWorld.Count} consumable items into database.",
            this,
            Logging.LogType.Info
        );

        consumableItemsDatabase.LoadConsumableItems(itemsFromWorld);
    }
}
