using System.Collections.Generic;
using FTRShared.Runtime.Models;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary
{
    public class CreatablesLibrary : IInitializable
    {
        private CreatablesData data;

        /// <summary>
        /// Loads the creatables data into the library.
        /// Should be called once by the creatables loader after loading a world.
        /// </summary>
        public void Load(CreatablesData loadedData) => data = loadedData;

        /// <summary>
        /// Returns a single creatable of type T matching the given id, or null if not found.
        ///
        /// <example>
        /// var enemy = creatablesLibrary.Get<EnemyData>("enemy-123");
        /// </example>
        ///
        /// </summary>
        public T Get<T>(string id)
            where T : class
        {
            var list = GetList<T>();
            return list?.Find(x => GetId(x) == id);
        }

        /// <summary>
        /// Returns all creatables of type T, or an empty list if none exist.
        /// <example>
        ///     var allEnemies = creatablesLibrary.GetAll<EnemyData>();
        /// </example>
        /// </summary>
        public List<T> GetAll<T>()
            where T : class
        {
            return GetList<T>() ?? new List<T>();
        }

        /// <summary>
        /// Maps a type T to its corresponding list in CreatablesData.
        /// Returns null if the type is not registered.
        /// <example>
        ///    var enemyList = GetList<EnemyData>();
        /// </example>
        /// </summary>
        private List<T> GetList<T>()
            where T : class
        {
            return typeof(T) switch
            {
                var t when t == typeof(EnemyData) => data.enemies as List<T>,
                var t when t == typeof(LootTableData) => data.lootTables as List<T>,
                var t when t == typeof(WeaponItemData) => data.weaponItems as List<T>,
                var t when t == typeof(ConsumableItemData) => data.consumableItems as List<T>,
                var t when t == typeof(NPCData) => data.npcs as List<T>,
                var t when t == typeof(QuestData) => data.quests as List<T>,
                _ => null,
            };
        }

        /// <summary>
        /// Extracts the id field from a creatable item.
        /// Required because data classes don't share a common interface with an id property.
        /// Consider implementing IIdentifiable across data classes to remove this method.
        /// </summary>
        private string GetId<T>(T item) =>
            item switch
            {
                EnemyData e => e.id,
                LootTableData l => l.id,
                WeaponItemData w => w.id,
                ConsumableItemData c => c.id,
                NPCData n => n.id,
                QuestData q => q.id,
                _ => null,
            };

        // IInitializable requires this method, but initialization is handled by Load().
        public void Initialize() { }
    }
}
