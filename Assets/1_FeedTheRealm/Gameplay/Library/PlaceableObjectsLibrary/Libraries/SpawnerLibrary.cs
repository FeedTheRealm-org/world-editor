using System.Collections.Generic;
using API;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.Library;
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
                SpawnerCategories.AggresiveNPC => UniTask.FromResult(
                    resolver.Instantiate(aggresiveNpcSpawnerPrefab)
                ),
                SpawnerCategories.FriendlyNPC => UniTask.FromResult(
                    resolver.Instantiate(friendlyNpcSpawnerPrefab)
                ),
                SpawnerCategories.PlayerSpawnpoint => UniTask.FromResult(
                    resolver.Instantiate(playerSpawnpointPrefab)
                ),
                _ => UniTask.FromResult<GameObject>(null),
            };
        }

        public List<string> ListAvailableItems()
        {
            return new List<string>
            {
                SpawnerCategories.AggresiveNPC,
                SpawnerCategories.FriendlyNPC,
                SpawnerCategories.PlayerSpawnpoint,
            };
        }
    }
}
