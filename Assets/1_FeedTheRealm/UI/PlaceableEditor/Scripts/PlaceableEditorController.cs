using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Core.WorldObjects.Provider;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.WorldEditor
{
    public class PlaceableEditorController : MonoBehaviour
    {
        [Inject]
        private Logging.Logger logger;

        [Inject]
        private EditPlaceableEvent editPlaceableEvent;

        [Inject]
        private WorldUIObjectProvider worldUIObjectProvider;

        [Inject]
        private IObjectResolver resolver;

        private Dictionary<PlaceableObjectCategories, GameObject> editorPrefabs = new();

        void Start()
        {
            editorPrefabs[PlaceableObjectCategories.Structure] =
                worldUIObjectProvider.structureEditObject;
            editorPrefabs[PlaceableObjectCategories.FriendlyNpcSpawner] =
                worldUIObjectProvider.FriendlyNpcSpawnerEditObject;
            editorPrefabs[PlaceableObjectCategories.AggresiveNpcSpawner] =
                worldUIObjectProvider.AggresiveNpcSpawnerEditObject;
            editorPrefabs[PlaceableObjectCategories.Portal] =
                worldUIObjectProvider.PortalEditObject;
            editorPrefabs[PlaceableObjectCategories.Chest] = worldUIObjectProvider.ChestEditObject;
            editorPrefabs[PlaceableObjectCategories.PlayerSpawnpointSpawner] =
                worldUIObjectProvider.PlayerSpawnerEditObject;

            editPlaceableEvent.OnRaised += HandleObjectSelected;
        }

        void OnDestroy()
        {
            editPlaceableEvent.OnRaised -= HandleObjectSelected;
        }

        private void HandleObjectSelected(EditableOption option)
        {
            if (!editorPrefabs.TryGetValue(option.category, out var prefab) || prefab == null)
            {
                logger.Log(
                    $"No editor prefab for category {option.category}",
                    this,
                    Logging.LogType.Error
                );
                return;
            }
            var instance = resolver.Instantiate(prefab);
            var editor = instance.GetComponent<IEditable>();
            if (editor == null)
            {
                logger.Log(
                    $"{prefab.name} has no IEditable component.",
                    this,
                    Logging.LogType.Error
                );
                Destroy(instance);
                return;
            }
            editor.Edit(option.placeable);
        }
    }
}
