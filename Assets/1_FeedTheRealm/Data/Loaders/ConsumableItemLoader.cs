using System.Collections.Generic;
using Models;
using UnityEngine;
using Utils;

[CreateAssetMenu(
    fileName = "ConsumableItemLoader",
    menuName = "Scriptable Objects/Loaders/ConsumableItemLoader"
)]
public class ConsumableItemLoader : ScriptableObject, ILoadable, ICreatableLoader
{
    [SerializeField]
    private Logging.Logger logger;

    private List<CreatorObject> consumableItems = new();

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
        logger.Log(
            $"Retrieving {consumableItems.Count} consumable items",
            this,
            Logging.LogType.Info
        );
        return consumableItems.FindAll(item => !item.IsDeleted);
    }

    public void AddCreatable(CreatorObject creatable)
    {
        consumableItems.Add(creatable);
        logger.Log(
            $"Added new creatable: {creatable.DisplayName} (ID: {creatable.ObjectId})",
            this,
            Logging.LogType.Info
        );
    }

    public void RemoveCreatable(CreatorObject creatable)
    {
        creatable.Delete();
        consumableItems.Remove(creatable);
    }

    public void UpdateCreatable(CreatorObject creatable)
    {
        int index = consumableItems.FindIndex(item => item.ObjectId == creatable.ObjectId);
        if (index != -1)
        {
            consumableItems[index] = creatable;
        }
    }

    public void LoadWorld(WorldData worldData)
    {
        consumableItems.Clear();
        if (worldData == null)
        {
            logger.Log("ItemLoader.LoadWorld: worldData is null.", this, Logging.LogType.Warning);
            return;
        }

        foreach (
            ConsumableItemData itemData in worldData.consumableItems
                ?? new List<ConsumableItemData>()
        )
        {
            consumableItems.Add(new ConsumableItem(itemData));
        }
    }
}
