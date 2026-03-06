using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FeedTheRealm.Core.WorldObjects.Items;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.Loaders.Items
{
    public abstract class ItemLoader<TItemData> : ScriptableObject, ILoadable, ICreatableLoader
    {
        [SerializeField]
        protected Logging.Logger logger;

        [SerializeField]
        private WorldSelectedEvent worldSelectedEvent;

        protected List<CreatorObject> items = new();

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
            logger.Log($"Retrieving {items.Count} items", this, Logging.LogType.Info);
            return items.FindAll(item => !item.IsDeleted);
        }

        public void AddCreatable(CreatorObject creatable)
        {
            items.Add(creatable);
            logger.Log(
                $"Added new creatable: {creatable.DisplayName} (ID: {creatable.ObjectId})",
                this,
                Logging.LogType.Info
            );
        }

        public void RemoveCreatable(CreatorObject creatable)
        {
            creatable.Delete();
            items.Remove(creatable);
        }

        public void UpdateCreatable(CreatorObject creatable)
        {
            int index = items.FindIndex(item => item.ObjectId == creatable.ObjectId);
            if (index != -1)
            {
                items[index] = creatable;
            }
        }

        public void LoadWorld(WorldData worldData)
        {
            items.Clear();
            if (worldData == null)
            {
                logger.Log(
                    "ItemLoader.LoadWorld: worldData is null.",
                    this,
                    Logging.LogType.Warning
                );
                return;
            }

            foreach (var itemData in GetData(worldData) ?? new List<TItemData>())
            {
                items.Add(CreateItem(itemData));
            }
        }

        protected abstract IEnumerable<TItemData> GetData(WorldData worldData);
        protected abstract CreatorObject CreateItem(TItemData data);
    }
}
