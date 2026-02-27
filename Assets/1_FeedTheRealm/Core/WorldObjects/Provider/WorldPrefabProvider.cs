using UnityEngine;

namespace FeedTheRealm.Core.WorldObjects.Provider
{
    [CreateAssetMenu(
        fileName = "WorldPrefabProvider",
        menuName = "Scriptable Objects/WorldPrefabProvider"
    )]
    public class WorldPrefabProvider : ScriptableObject
    {
        public GameObject playerPrefab;
        public GameObject worldPrefab;
    }
}
