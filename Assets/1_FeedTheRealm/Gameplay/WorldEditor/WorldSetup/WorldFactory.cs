using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class WorldFactory
    {
        private readonly WorldControllerV2 worldController;

        public WorldFactory(WorldControllerV2 worldPrefab)
        {
            this.worldController = worldPrefab;
        }

        public GameObject Create()
        {
            WorldControllerV2 worldInstance = Object.Instantiate(worldController);
            worldInstance.gameObject.name = "World";
            worldInstance.gameObject.transform.position = Vector3.zero;
            return worldInstance.gameObject;
        }
    }
}
