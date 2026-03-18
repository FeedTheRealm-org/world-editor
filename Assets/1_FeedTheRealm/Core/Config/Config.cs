using System.IO;
using UnityEngine;

namespace FTR.Core.Common.Config
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Config/Config")]
    public class Config : ScriptableObject
    {
        [Header("Reinitialization Options")]
        [SerializeField]
        private bool forceReinitialize = false;
        public bool ForceReinitialize => forceReinitialize;

        [Header("Persistent Model file config")]
        [SerializeField]
        private string modelsDataFile = "Models/models.json";

        [SerializeField]
        private string modelsDirectory = "Models";

        [Header("Persistent World file config")]
        [SerializeField]
        private string worldsDirectory = "Worlds";

        [SerializeField]
        private string worldsFileExtension = ".world";

        [SerializeField]
        private string zoneFilePrefix = "zone_";

        public string ModelsDataFile =>
            Path.Combine(Application.persistentDataPath, modelsDataFile);
        public string ModelsDirectory =>
            Path.Combine(Application.streamingAssetsPath, modelsDirectory);
        public string WorldDirectory =>
            Path.Combine(Application.persistentDataPath, worldsDirectory);
        public string WorldFileExtension => worldsFileExtension;
        public string ZoneFilePrefix => zoneFilePrefix;
    }
}
