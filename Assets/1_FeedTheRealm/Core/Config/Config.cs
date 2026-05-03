using System.IO;
using UnityEngine;

namespace FTR.Core.Common.Config
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Config/Config")]
    public class Config : ScriptableObject
    {
        [Header("Reinitialization Options")]
        [SerializeField]
        public bool ForceReinitialize = false;

        [Header("Persistent Model file config")]
        [SerializeField]
        private string modelsDataDirectory = "Models";

        [SerializeField]
        private string modelsDirectory = "Models";

        [Header("Persistent World file config")]
        [SerializeField]
        private string worldsDirectory = "Worlds";

        [SerializeField]
        private string spritesDirectory = "Sprites";

        [SerializeField]
        private string worldsFileExtension = ".world";

        [SerializeField]
        private string worldDataFileName = "world.data";

        [SerializeField]
        private string creatablesFileName = "creatables.data";

        [SerializeField]
        private string zoneFilePrefix = "zone_";

        [Header("UI Configs")]
        public string defaultOpenChestId;
        public string defaultClosedChestId;

        public string ModelsDataDirectory =>
            Path.Combine(Application.persistentDataPath, modelsDataDirectory);
        public string ModelsDirectory =>
            Path.Combine(Application.streamingAssetsPath, modelsDirectory);
        public string SpritesDirectory =>
            Path.Combine(Application.streamingAssetsPath, spritesDirectory);
        public string WorldDirectory =>
            Path.Combine(Application.persistentDataPath, worldsDirectory);
        public string WorldFileExtension => worldsFileExtension;
        public string WorldDataFilename => worldDataFileName;
        public string CreatablesFileName => creatablesFileName;
        public string ZoneFilePrefix => zoneFilePrefix;
    }
}
