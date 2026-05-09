using UnityEngine;

namespace FeedTheRealm.Core.WorldObjects.Provider
{
    [CreateAssetMenu(
        fileName = "WorldPrefabProvider",
        menuName = "Scriptable Objects/Providers/WorldPrefabProvider"
    )]
    public class WorldPrefabProvider : ScriptableObject
    {
        public GameObject playerPrefab;
        public GameObject worldEditorPrefab;

        [Header("World Settings")]
        public GameObject worldPrefab;

        [Header("Placeable Objects")]
        public GameObject structurePrefab;
        public GameObject aggresiveNPCSpawnerPrefab;
        public GameObject friendlyNPCSpawnerPrefab;
        public GameObject playerSpawnpointPrefab;
        public GameObject portalPrefab;
        public GameObject chestPrefab;

        [Header("Error Handling")]
        public GameObject errorPrefab;
    }
}
