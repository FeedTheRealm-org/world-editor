using System.Collections.Generic;
using System.Linq;
using API;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Core.WorldObjects.Provider;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary
{
    public class SpawnerLibrary : ILibrary
    {
        private GameObject aggresiveNpcSpawnerPrefab;
        private GameObject friendlyNpcSpawnerPrefab;
        private GameObject playerSpawnpointPrefab;
        private IObjectResolver resolver;

        public SpawnerLibrary(WorldPrefabProvider prefabProvider, IObjectResolver resolver)
        {
            aggresiveNpcSpawnerPrefab = prefabProvider.aggresiveNPCSpawnerPrefab;
            friendlyNpcSpawnerPrefab = prefabProvider.friendlyNPCSpawnerPrefab;
            playerSpawnpointPrefab = prefabProvider.playerSpawnpointPrefab;
            this.resolver = resolver;
        }

        public UniTask<GameObject> GetItem(string itemName)
        {
            return itemName switch
            {
                SpawnerTypes.AggresiveNPC => UniTask.FromResult(
                    resolver.Instantiate(aggresiveNpcSpawnerPrefab)
                ),
                SpawnerTypes.FriendlyNPC => UniTask.FromResult(
                    resolver.Instantiate(friendlyNpcSpawnerPrefab)
                ),
                SpawnerTypes.PlayerSpawnpoint => UniTask.FromResult(
                    resolver.Instantiate(playerSpawnpointPrefab)
                ),
                _ => UniTask.FromResult<GameObject>(null),
            };
        }

        public List<PlaceableOption> ListAvailableItems()
        {
            return new List<PlaceableOption>
            {
                new()
                {
                    category = PlaceableObjectCategories.Spawner,
                    id = SpawnerTypes.AggresiveNPC,
                    displayName = SpawnerTypes.AggresiveNPC,
                },
                new()
                {
                    category = PlaceableObjectCategories.Spawner,
                    id = SpawnerTypes.FriendlyNPC,
                    displayName = SpawnerTypes.FriendlyNPC,
                },
                new()
                {
                    category = PlaceableObjectCategories.Spawner,
                    id = SpawnerTypes.PlayerSpawnpoint,
                    displayName = SpawnerTypes.PlayerSpawnpoint,
                },
            };
        }
    }
}
