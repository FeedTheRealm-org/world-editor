using System;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.Repository;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.WorldLoader
{
    public class CreatableLoader
    {
        private Logging.Logger logger;
        private CreatablesManager creatablesManager;

        public CreatableLoader(Logging.Logger logger, CreatablesManager creatablesManager)
        {
            this.logger = logger;
            this.creatablesManager = creatablesManager;
        }

        /// <summary>
        /// Loads all creatables for the specified world. This includes enemies, loot tables, weapons, consumables, NPCs, and quests.}
        /// The loaded creatables are registered in the CreatablesManager for use throughout the game.
        /// To avoid redundant loading of creatables in the same world but different zone, the loader checks if the creatables for the specified world are already loaded in the CreatablesManager.
        /// If they are, it skips the loading process.
        /// </summary>
        public UniTask Load(string worldId, CreatablesData data)
        {
            if (creatablesManager.CurrentWorldId == worldId)
                return UniTask.CompletedTask;

            creatablesManager.ClearRegistry();
            creatablesManager.CurrentWorldId = worldId;

            foreach (var enemy in data.enemies)
                creatablesManager.Add(new AggresiveNpc(enemy));

            foreach (var lootTable in data.lootTables)
                creatablesManager.Add(new LootTable(lootTable));

            foreach (var weapon in data.weaponItems)
                creatablesManager.Add(new Weapon(weapon));

            foreach (var consumable in data.consumableItems)
                creatablesManager.Add(new ConsumableItem(consumable));

            foreach (var npc in data.npcs)
                creatablesManager.Add(new FriendlyNpc(npc));

            foreach (var quest in data.quests)
                creatablesManager.Add(new Quest(quest));

            logger.Log("[CreatableLoader] Creatables loaded successfully.");
            return UniTask.CompletedTask;
        }
    }
}
