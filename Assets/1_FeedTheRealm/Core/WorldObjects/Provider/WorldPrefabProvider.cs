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
        public LayerMask worldLayerMask;
    }
}
