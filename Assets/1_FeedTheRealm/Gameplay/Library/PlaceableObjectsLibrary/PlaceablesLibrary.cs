using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary
{
    public class PlaceablesLibrary : IInitializable
    {
        private readonly Logging.Logger logger;
        private Dictionary<PlaceableObjectCategories, ILibrary> library = new();

        public PlaceablesLibrary(
            Logging.Logger logger,
            StructureLibrary structureLibrary,
            SpawnerLibrary spawnerLibrary,
            MiscLibrary miscLibrary
        )
        {
            this.logger = logger;
            library[PlaceableObjectCategories.Structure] = structureLibrary;
            library[PlaceableObjectCategories.Spawner] = spawnerLibrary;
            library[PlaceableObjectCategories.Misc] = miscLibrary;
        }

        // IInitializable requiers this method, but we don't need to do anything on initialization for this repository
        // We implement this interface just to ensure that the repository is created when registered.
        public void Initialize() { }

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
