using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using UnityEngine;

namespace FeedTheRealm.Gameplay.Library
{
    [CreateAssetMenu(
        fileName = "CreatablesManager",
        menuName = "Scriptable Objects/CreatablesManager"
    )]
    /// We make the CreatablesManager a ScriptableObject so its data
    /// can be persisted across zone loads in runtime, allowing creatables to be shared
    ///  across zones in the same world
    public class CreatablesManager : ScriptableObject
    {
        private Dictionary<CreatableObjectCategories, List<ICreatable>> registry = new();

        [SerializeField]
        private CreatablesDataRegistryEvent registryEvent;

        [SerializeField]
        private Logging.Logger logger;

        public string CurrentWorldId { get; set; }

        public void ClearRegistry()
        {
            registry.Clear();
            logger.Log($"[CreatablesManager] Registry cleared.");
        }

        /// <summary>
        /// Adds a creatable to the registry under its category.
        /// </summary>
        public void Add(ICreatable creatable)
        {
            var category = creatable.Category;
            if (!registry.ContainsKey(category))
                registry[category] = new List<ICreatable>();

            registry[category].Add(creatable);
            registryEvent.Raise(creatable);
            logger.Log(
                $"[CreatablesManager] Added and registered {creatable.GetType().Name} under {category}"
            );
        }

        // ---- Get ----

        /// <summary>
        /// Returns all creatables of a given category.
        /// </summary>
        public List<ICreatable> GetAll(CreatableObjectCategories category)
        {
            if (!registry.ContainsKey(category))
                return new List<ICreatable>();
            return registry[category];
        }

        // ---- Delete ----

        /// <summary>
        /// Removes a creatable by id from the registry.
        /// </summary>
        public void Delete(CreatableObjectCategories category, string id)
        {
            if (!registry.ContainsKey(category))
            {
                logger.Log(
                    $"[CreatablesManager] Category {category} not found.",
                    Logging.LogType.Warning
                );
                return;
            }
            var removed = registry[category].RemoveAll(x => x.Id == id);
            if (removed == 0)
                logger.Log(
                    $"[CreatablesManager] Could not find object with id {id} to delete.",
                    Logging.LogType.Warning
                );
        }
    }
}
