using UnityEngine;

namespace FeedTheRealm.Core.DataPersistence
{
    [CreateAssetMenu(fileName = "WorldSelector", menuName = "Scriptable Objects/World Selector")]
    public class WorldSelector : ScriptableObject
    {
        public string selectedWorld;
        private int _selectedZoneId = 1;
        public int selectedZoneId
        {
            get => _selectedZoneId < 1 ? 1 : _selectedZoneId;
            set => _selectedZoneId = value < 1 ? 1 : value;
        }
        public string selectedWorldId = "";

        public void ClearSelection()
        {
            selectedWorld = null;
            selectedZoneId = 1;
            selectedWorldId = "";
        }
    }
}
