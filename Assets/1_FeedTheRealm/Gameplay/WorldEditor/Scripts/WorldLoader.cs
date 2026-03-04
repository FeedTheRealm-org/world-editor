using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using Models;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Gameplay.WorldEditor
{
    public class WorldLoader
    {
        private readonly DataPersistenceManagerSO dataPersistenceManager;
        private readonly WorldSelectedEvent worldSelectedEvent;

        public WorldLoader(
            DataPersistenceManagerSO dataPersistenceManager,
            WorldSelectedEvent worldSelectedEvent
        )
        {
            this.dataPersistenceManager = dataPersistenceManager;
            this.worldSelectedEvent = worldSelectedEvent;
        }

        public void Load()
        {
            WorldData worldData = dataPersistenceManager.CurrentWorldData;
            LoadWorld(worldData);
        }

        // TODO: consider adding a loading screen or something to avoid having the user
        // see how the world is being populated.
        public void LoadWorld(WorldData worldData)
        {
            worldSelectedEvent.Raise(worldData);
        }
    }
}
