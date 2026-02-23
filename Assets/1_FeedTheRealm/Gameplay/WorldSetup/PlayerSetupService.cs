using FeedTheRealm.Core.Interfaces;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class PlayerSetupService : ISetup
    {
        private readonly GameObject playerPrefab;

        public PlayerSetupService(WorldPrefabProvider worldPrefabProvider)
        {
            if (worldPrefabProvider == null)
            {
                Debug.LogError("World prefab not set!");
                return;
            }
            playerPrefab = worldPrefabProvider.playerPrefab;
        }

        public void Setup()
        {
            playerPrefab.name = "Player";
            Object.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}
