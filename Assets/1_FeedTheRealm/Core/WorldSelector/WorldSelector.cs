using UnityEngine;

namespace FeedTheRealm.Core.DataPersistence
{
    [CreateAssetMenu(fileName = "WorldSelector", menuName = "Scriptable Objects/World Selector")]
    public class WorldSelector : ScriptableObject
    {
        public string selectedWorld;
        public int selectedZoneId = 1;
    }
}
