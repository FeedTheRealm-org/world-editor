using System;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.Repository;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.WorldLoader
{
    public class CreatablesLoader
    {
        private Logging.Logger logger;
        private CreatablesManager creatablesManager;
        private readonly WorldSelector worldSelector;
        private readonly DataPersistenceManager dataPersistenceManager;

        public CreatablesLoader(
            Logging.Logger logger,
            CreatablesManager creatablesManager,
            WorldSelector worldSelector,
            DataPersistenceManager dataPersistenceManager
        )
        {
            this.logger = logger;
            this.creatablesManager = creatablesManager;
            this.worldSelector = worldSelector;
            this.dataPersistenceManager = dataPersistenceManager;
        }

        /// <summary>
        /// Loads all creatables for the specified world. This includes enemies, loot tables, weapons, consumables, NPCs, and quests.}
        /// The loaded creatables are registered in the CreatablesManager for use throughout the game.
        /// </summary>
        public async UniTask Load()
        {
            try
            {
                creatablesManager.ClearRegistry();

                if (string.IsNullOrEmpty(worldSelector.selectedWorld))
                    return;

                CreatablesData data = dataPersistenceManager.GetCreatables(
                    worldSelector.selectedWorld
                );
                if (data == null)
                    return;

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

                foreach (var dialog in data.dialogs)
                    creatablesManager.Add(new Dialog(dialog));

                foreach (var shop in data.shops)
                    creatablesManager.Add(new Shop(shop));

                logger.Log("[CreatableLoader] Creatables loaded successfully.");
            }
            catch (Exception ex)
            {
                logger.Log($"Error loading creatables: {ex.Message}");
            }
        }
    }
}
