using System;
using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FeedTheRealm.Gameplay.Creatables;
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
        private Dictionary<Type, List<ICreatable>> registry = new();

        [SerializeField]
        private CreatablesDataRegistryEvent registryEvent;

        [SerializeField]
        private Logging.Logger logger;

        public string CurrentWorldId { get; set; }

        public void ClearRegistry()
        {
            registry.Clear();
            logger.Log("[CreatablesManager] Registry cleared.");
        }

        /// <summary>
        /// Adds a creatable to the registry and raises an event to notify listeners of the new addition.
        /// </summary>
        public void Add(ICreatable creatable)
        {
            var type = creatable.GetType();
            if (!registry.ContainsKey(type))
                registry[type] = new List<ICreatable>();

            registry[type].Add(creatable);
            registryEvent.Raise(creatable);
            logger.Log($"[CreatablesManager] Added {type.Name}");
        }

        /// <summary>
        ///  Returns all creatables of type T in the registry. If no creatables of that type are found, returns an empty list.
        ///  Example:
        ///     var weapons = GetAll<WeaponItem>();
        ///
        ///  When Getting a creatable, they are reference types, so they can be modified and those modifications will persist in the registry.
        ///  This allows you to get a creatable, modify its properties and have those changes reflected in the registry without needing to re-add it.
        ///
        /// </summary>
        public List<T> GetAll<T>()
            where T : class, ICreatable
        {
            if (!registry.ContainsKey(typeof(T)))
                return new List<T>();

            return registry[typeof(T)].OfType<T>().ToList();
        }

        /// <summary>
        /// Removes a creatable with the specified id from the registry. If no creatable with that id is found, logs a warning.
        /// Example:
        ///     Delete<WeaponItem>("sword_001");
        /// </summary>
        public void Delete<T>(string id)
            where T : class, ICreatable
        {
            if (!registry.ContainsKey(typeof(T)))
            {
                logger.Log(
                    $"[CreatablesManager] No entries found for {typeof(T).Name}.",
                    Logging.LogType.Warning
                );
                return;
            }

            var removed = registry[typeof(T)].RemoveAll(x => x.Id == id);
            if (removed == 0)
                logger.Log(
                    $"[CreatablesManager] Could not find id {id} in {typeof(T).Name}.",
                    Logging.LogType.Warning
                );
        }
    }
}
