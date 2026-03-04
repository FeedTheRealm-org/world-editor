using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API;
using FeedTheRealm.Gameplay.Library.CreatorObjectLibrary;
using FeedTheRealm.Gameplay.Loaders;
using Models;
using UnityEngine;

namespace FeedTheRealm.UI.MenuBar.FileOption.PublishMenu
{
    public class WorldPublisherController : MonoBehaviour
    {
        [SerializeField]
        Logging.Logger logger;

        [SerializeField]
        StructureLoaderSO structureLoader;

        [SerializeField]
        CreatorObjectLibrarySO creatorObjectLibrary;

        [SerializeField]
        WorldService worldService;

        [SerializeField]
        SpriteService spriteService;

        [SerializeField]
        ModelService modelService;

        [SerializeField]
        ItemService itemService;

        [SerializeField]
        Session.Session session;

        /// <summary>
        /// Publishes the world, its models and sprites to the server.
        /// If any step fails, returns the error message.
        /// </summary>
        public async Task<(string, string, long)> PublishWorld(
            WorldData worldData,
            string worldFile,
            string description
        )
        {
            (string worldId, string worldError, long statusCode) = await worldService.PublishWorld(
                worldData,
                worldFile,
                description,
                session.APIToken
            );
            if (!string.IsNullOrEmpty(worldError))
                return (null, worldError, statusCode);

            string modelError = await PublishModels(worldData, worldId);
            if (!string.IsNullOrEmpty(modelError))
                return (null, modelError, 0);

            var consumableSpriteData = new Dictionary<string, string>();
            if (worldData.consumableItems != null)
            {
                foreach (var item in worldData.consumableItems)
                {
                    if (
                        !string.IsNullOrEmpty(item.spriteFilePath)
                        && !consumableSpriteData.ContainsKey(item.id)
                    )
                        consumableSpriteData[item.id] = item.spriteFilePath;
                }
            }

            var weaponSpriteData = new Dictionary<string, string>();
            if (worldData.weaponItems != null)
            {
                foreach (var item in worldData.weaponItems)
                {
                    if (
                        !string.IsNullOrEmpty(item.spriteFilePath)
                        && !weaponSpriteData.ContainsKey(item.id)
                    )
                        weaponSpriteData[item.id] = item.spriteFilePath;
                }
            }

            ItemCategoryListResponse categoryListResponse = await itemService.GetItemCategories(
                session.APIToken
            );
            Debug.Log($"Fetched {categoryListResponse.category_list} item categories from server.");

            ItemCategoryResponse consumableCategory =
                categoryListResponse.category_list.FirstOrDefault(c =>
                    c.category_name == "consumables"
                );
            ItemCategoryResponse weaponCategory = categoryListResponse.category_list.FirstOrDefault(
                c => c.category_name == "weapons"
            );

            if (consumableCategory == null)
            {
                return (null, "Item category 'consumables' not found on server.", 0);
            }

            if (weaponCategory == null)
            {
                return (null, "Item category 'weapons' not found on server.", 0);
            }

            var consumableSpriteList = consumableSpriteData
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToList();
            string consumableSpriteError = await PublishItems(
                consumableSpriteList,
                worldId,
                consumableCategory.category_id
            );
            if (!string.IsNullOrEmpty(consumableSpriteError))
                return (null, consumableSpriteError, 0);

            var weaponSpriteList = weaponSpriteData.Select(kvp => (kvp.Key, kvp.Value)).ToList();
            string weaponSpriteError = await PublishItems(
                weaponSpriteList,
                worldId,
                weaponCategory.category_id
            );
            if (!string.IsNullOrEmpty(weaponSpriteError))
                return (null, weaponSpriteError, 0);

            return (worldId, null, 200);
        }

        /// <summary>
        ///  Uploads model files for a world.
        ///  If an error occurs, returns the error message.
        /// </summary>
        private async Task<string> PublishModels(WorldData worldData, string worldId)
        {
            if (worldData.objectPlacementData.Count == 0)
                return null;

            Debug.Log(
                $"Publishing models for world {worldId}, total structures: {worldData.objectPlacementData.Count}"
            );

            var uniqueStructures = worldData
                .objectPlacementData.GroupBy(s => s.id)
                .Select(g => g.First())
                .ToList();

            foreach (var structure in uniqueStructures)
            {
                structure.structureFilepath = structureLoader.GetModelFilePath(
                    structure.structureName
                );
            }
            return await modelService.UploadModels(uniqueStructures, worldId, session.APIToken);
        }

        /// <summary>
        ///  Uploads model files for a world.
        ///  If an error occurs, returns the error message.
        /// </summary>
        private async Task<string> PublishItems(
            List<(string, string)> itemData,
            string worldId,
            string categoryId
        )
        {
            return await itemService.UploadItemsByCategory(
                itemData,
                worldId,
                categoryId,
                session.APIToken
            );
        }
    }
}
