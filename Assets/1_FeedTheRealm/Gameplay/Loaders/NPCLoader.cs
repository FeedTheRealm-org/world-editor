using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels;
using Models;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCLoader", menuName = "Scriptable Objects/Loaders/NPCLoader")]
public class NPCLoader : ScriptableObject, ILoadable, ICreatableLoader
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private WorldSelectedEvent worldSelectedEvent;

    private List<CreatorObject> npcs = new();

    void OnEnable()
    {
        worldSelectedEvent.OnRaised += LoadWorld;
    }

    void OnDisable()
    {
        worldSelectedEvent.OnRaised -= LoadWorld;
    }

    public List<CreatorObject> GetCreatables()
    {
        logger?.Log(
            $"NPCLoader: Retrieving {npcs.Count} NPCs (filtered: {npcs.FindAll(item => !item.IsDeleted).Count})",
            this,
            Logging.LogType.Info
        );
        return npcs.FindAll(item => !item.IsDeleted);
    }

    public void AddCreatable(CreatorObject creatable)
    {
        npcs.Add(creatable);
        logger?.Log(
            $"NPCLoader: Added NPC '{creatable.DisplayName}' (ID: {creatable.ObjectId}). Total NPCs: {npcs.Count}",
            this,
            Logging.LogType.Info
        );
    }

    public void RemoveCreatable(CreatorObject creatable)
    {
        creatable.Delete();
        npcs.Remove(creatable);
    }

    public void UpdateCreatable(CreatorObject creatable)
    {
        int index = npcs.FindIndex(item => item.ObjectId == creatable.ObjectId);
        if (index != -1)
        {
            npcs[index] = creatable;
        }
    }

    public void LoadWorld(WorldData worldData)
    {
        npcs.Clear();
        if (worldData == null)
        {
            logger.Log("NPCLoader.LoadWorld: worldData is null.", this, Logging.LogType.Warning);
            return;
        }

        logger?.Log(
            $"NPCLoader: Loading {worldData.npcs?.Count ?? 0} NPCs from WorldData",
            this,
            Logging.LogType.Info
        );

        foreach (NPCData npcData in worldData.npcs ?? new List<NPCData>())
        {
            var npc = new GenericNPC(npcData);
            npcs.Add(npc);
            logger?.Log(
                $"NPCLoader: Loaded NPC '{npc.DisplayName}' (ID: {npc.ObjectId})",
                this,
                Logging.LogType.Info
            );
        }

        logger?.Log(
            $"NPCLoader: Finished loading. Total NPCs in memory: {npcs.Count}",
            this,
            Logging.LogType.Info
        );
    }
}
