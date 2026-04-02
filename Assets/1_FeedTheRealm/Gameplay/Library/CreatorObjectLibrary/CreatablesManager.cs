using System;
using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects;
using VContainer;

namespace FeedTheRealm.Gameplay.Library
{
    public class CreatablesManager
    {
        private CreatablesDataRegistryEvent registryEvent;
        private Logging.Logger logger;
        private IObjectResolver container;
        private Dictionary<Type, List<Creatable>> registry = new();

        public CreatablesManager(
            IObjectResolver container,
            CreatablesDataRegistryEvent registryEvent,
            Logging.Logger logger
        )
        {
            this.container = container;
            this.registryEvent = registryEvent;
            this.logger = logger;
        }

        public void ClearRegistry()
        {
            registry.Clear();
            logger.Log("[CreatablesManager] Registry cleared.");
        }

        /// <summary>
        /// Adds a creatable to the registry and raises an event to notify listeners of the new addition.
        /// </summary>
        public void Add(Creatable creatable)
        {
            var type = creatable.GetType();

            if (!registry.ContainsKey(type))
                registry[type] = new List<Creatable>();

            container.Inject(creatable);
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
            where T : Creatable
        {
            if (!registry.ContainsKey(typeof(T)))
                return new List<T>();
            return registry[typeof(T)].OfType<T>().Where(c => !c.IsDeleted).ToList();
        }

        /// <summary>
        /// Removes a creatable with the specified id from the registry. If no creatable with that id is found, logs a warning.
        /// Example:
        ///     Delete<WeaponItem>("sword_001");
        /// </summary>
        public void Delete<T>(string id)
            where T : Creatable
        {
            if (!registry.ContainsKey(typeof(T)))
            {
                logger.Log(
                    $"[CreatablesManager] No entries found for {typeof(T).Name}.",
                    Logging.LogType.Warning
                );
                return;
            }

            var toBeDeletedItem = registry[typeof(T)].OfType<T>().FirstOrDefault(c => c.Id == id);
            if (toBeDeletedItem == null)
            {
                logger.Log(
                    $"[CreatablesManager] No {typeof(T).Name} found with id '{id}'.",
                    Logging.LogType.Warning
                );
                return;
            }
            toBeDeletedItem.Delete();
        }
    }
}
