using System.Collections.Generic;
using Models;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ConsumableItemLibrary",
    menuName = "Scriptable Objects/Library/ConsumableItemLibrary"
)]
public class ConsumableItemLibrarySO : ScriptableObject
{
    [SerializeField]
    private Logging.Logger logger;

    [Header("List of Consumable Items")]
    [SerializeField]
    private List<ConsumableItem> consumableItems = new List<ConsumableItem>();

    public void AddConsumableItem(ConsumableItem item)
    {
        if (consumableItems == null)
            consumableItems = new List<ConsumableItem>();
        logger.Log($"Adding consumable item: {item.name}", this, Logging.LogType.Info);
        consumableItems.Add(item);
    }

    public List<ConsumableItem> GetAllConsumableItems()
    {
        if (consumableItems == null)
            return new List<ConsumableItem>();
        return consumableItems;
    }

    public void RemoveConsumableItem(ConsumableItem item)
    {
        if (consumableItems == null)
            return;
        if (consumableItems.Remove(item))
        {
            logger.Log($"Removed consumable item: {item.name}", this, Logging.LogType.Info);
        }
        else
        {
            logger.Log(
                $"Failed to remove consumable item (not found): {item.name}",
                this,
                Logging.LogType.Warning
            );
        }
    }

    public void LoadConsumableItems(List<ConsumableItem> itemsToLoad)
    {
        if (consumableItems == null)
            consumableItems = new List<ConsumableItem>();
        consumableItems.Clear();
        consumableItems.AddRange(itemsToLoad);
        logger.Log(
            $"Loaded {itemsToLoad.Count} consumable items from world data.",
            this,
            Logging.LogType.Info
        );
    }
}
