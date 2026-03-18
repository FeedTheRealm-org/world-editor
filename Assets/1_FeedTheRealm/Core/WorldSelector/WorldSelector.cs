using UnityEngine;

namespace FeedTheRealm.Core.DataPersistence
{
    [CreateAssetMenu(fileName = "WorldSelector", menuName = "Scriptable Objects/World Selector")]
    public class WorldSelector : ScriptableObject
    {
        [SerializeField]
        public string selectedWorld;

        [SerializeField]
        public int selectedZoneId = 1;
    }
}
