using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class PlayerSetupService : SetupService
    {
        private readonly GameObject playerPrefab;
        private readonly IObjectResolver objectResolver;
        private readonly PlayerConfig playerConfig;

        public PlayerSetupService(
            WorldPrefabProvider worldPrefabProvider,
            IObjectResolver objectResolver,
            PlayerConfig playerConfig,
            WorldSetupEvent setupEvent
        )
            : base(setupEvent)
        {
            if (worldPrefabProvider == null)
            {
                Debug.LogError("World prefab not set!");
                return;
            }
            playerPrefab = worldPrefabProvider.playerPrefab;
            this.objectResolver = objectResolver;
            this.playerConfig = playerConfig;
        }

        public override void Setup()
        {
            GameObject playerInstance = Object.Instantiate(
                playerPrefab,
                new Vector3(playerConfig.positionX, playerConfig.positionY, playerConfig.positionZ),
                Quaternion.identity
            );
            playerInstance.name = "Player";
            playerInstance.SetActive(false);
            objectResolver.InjectGameObject(playerInstance);
            playerInstance.SetActive(true);
        }
    }
}
