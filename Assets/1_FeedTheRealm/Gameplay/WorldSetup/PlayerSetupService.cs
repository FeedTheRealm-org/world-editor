using FeedTheRealm.Core.Interfaces;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class PlayerSetupService : ISetup
    {
        private readonly GameObject playerPrefab;
        private readonly IObjectResolver objectResolver;

        public PlayerSetupService(
            WorldPrefabProvider worldPrefabProvider,
            IObjectResolver objectResolver
        )
        {
            if (worldPrefabProvider == null)
            {
                Debug.LogError("World prefab not set!");
                return;
            }
            playerPrefab = worldPrefabProvider.playerPrefab;
            this.objectResolver = objectResolver;
        }

        public void Setup()
        {
            playerPrefab.name = "Player";
            playerPrefab.SetActive(false);
            GameObject playerInstance = Object.Instantiate(
                playerPrefab,
                Vector3.zero,
                Quaternion.identity
            );
            objectResolver.InjectGameObject(playerInstance);
            playerInstance.SetActive(true);
        }
    }
}
