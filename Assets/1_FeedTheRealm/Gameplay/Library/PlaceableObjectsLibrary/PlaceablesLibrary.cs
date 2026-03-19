using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldEditor;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary
{
    public class PlaceablesLibrary
    {
        private Logging.Logger logger;
        private Dictionary<PlaceableObjectCategories, ILibrary> library = new();

        public PlaceablesLibrary(Logging.Logger logger, IObjectResolver objectResolver)
        {
            this.logger = logger;
            var structureLibrary = objectResolver.Resolve<StructureLibrary>();
            var spawnerLibrary = objectResolver.Resolve<SpawnerLibrary>();
            library[PlaceableObjectCategories.Structure] = structureLibrary;
            library[PlaceableObjectCategories.Spawner] = spawnerLibrary;
            logger.Log(
                $"PlaceablesLibrary initialized with categories: {string.Join(", ", library.Keys)}",
                Logging.LogType.Info
            );
        }

        public List<PlaceableOption> GetPlaceableOptions(PlaceableObjectCategories category)
        {
            if (!library.ContainsKey(category))
            {
                logger.Log(
                    $"Category {category} not found in Placeable Objects Library",
                    Logging.LogType.Error
                );
                return new List<PlaceableOption>();
            }
            return library[category].ListAvailableItems();
        }

        public UniTask<GameObject> GetObject(PlaceableObjectCategories category, string id)
        {
            if (!library.ContainsKey(category))
            {
                logger.Log(
                    $"Cannot find object with ID {id} in category {category} in Placeable Objects Library",
                    Logging.LogType.Error
                );
                return UniTask.FromResult<GameObject>(null);
            }
            return library[category].GetItem(id);
        }
    }
}
