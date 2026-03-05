using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FeedTheRealm.Core.WorldObjects.Quests;
using Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.Loaders
{
    [CreateAssetMenu(fileName = "QuestLoader", menuName = "Scriptable Objects/Loaders/QuestLoader")]
    public class QuestLoader : ScriptableObject, ILoadable, ICreatableLoader
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private WorldSelectedEvent worldSelectedEvent;

        private List<CreatorObject> quests = new();

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
                $"QuestLoader: Retrieving {quests.Count} quests (filtered: {quests.FindAll(item => !item.IsDeleted).Count})",
                this,
                Logging.LogType.Info
            );
            return quests.FindAll(item => !item.IsDeleted);
        }

        public void AddCreatable(CreatorObject creatable)
        {
            quests.Add(creatable);
            logger?.Log(
                $"QuestLoader: Added quest '{creatable.DisplayName}' (ID: {creatable.ObjectId}). Total quests: {quests.Count}",
                this,
                Logging.LogType.Info
            );
        }

        public void RemoveCreatable(CreatorObject creatable)
        {
            creatable.Delete();
            quests.Remove(creatable);
        }

        public void UpdateCreatable(CreatorObject creatable)
        {
            int index = quests.FindIndex(item => item.ObjectId == creatable.ObjectId);
            if (index != -1)
            {
                quests[index] = creatable;
            }
        }

        public void LoadWorld(WorldData worldData)
        {
            quests.Clear();
            if (worldData == null)
            {
                logger.Log(
                    "QuestLoader.LoadWorld: worldData is null.",
                    this,
                    Logging.LogType.Warning
                );
                return;
            }

            logger?.Log(
                $"QuestLoader: Loading {worldData.quests?.Count ?? 0} quests from WorldData",
                this,
                Logging.LogType.Info
            );

            foreach (QuestData questData in worldData.quests ?? new List<QuestData>())
            {
                var quest = new GenericQuest(questData);
                quests.Add(quest);
                logger?.Log(
                    $"QuestLoader: Loaded quest '{quest.DisplayName}' (ID: {quest.ObjectId})",
                    this,
                    Logging.LogType.Info
                );
            }

            logger?.Log(
                $"QuestLoader: Finished loading. Total quests in memory: {quests.Count}",
                this,
                Logging.LogType.Info
            );
        }
    }
}
