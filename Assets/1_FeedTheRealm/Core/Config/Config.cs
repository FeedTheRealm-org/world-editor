using System.IO;
using UnityEngine;

namespace FTR.Core.Common.Config
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Config/Config")]
    public class Config : ScriptableObject
    {
        [SerializeField]
        private ApiConfig apiConfig;

        [Header("Environment Config")]
        [SerializeField]
        private string devDirectory = "Dev";

        [SerializeField]
        private string prodDirectory = "Prod";

        [Header("Assets directories")]
        [SerializeField]
        private string modelsDataDirectory = "ModelsData";

        [SerializeField]
        private string spritesDirectory = "Sprites";

        [SerializeField]
        private string materialsDirectory = "Materials";

        [Header("Persistent Models data directory")]
        [SerializeField]
        private string modelsDirectory = "Models";

        [Header("Persistent World file")]
        [SerializeField]
        private string worldsDirectory = "Worlds";

        [Header("File Naming")]
        [SerializeField]
        private string worldsFileExtension = ".world";

        [SerializeField]
        private string worldDataFileName = "world.data";

        [SerializeField]
        private string creatablesFileName = "creatables.data";

        [SerializeField]
        private string zoneFilePrefix = "zone_";

        [Header("Layer Masks")]
        [SerializeField]
        private LayerMask placeableLayerMask;

        [SerializeField]
        private LayerMask worldObjectLayerMask;

        [Header("Default Chest Config")]
        public string defaultOpenChestId;
        public string defaultClosedChestId;

        [Header("Default Materials")]
        [SerializeField]
        public Material defaultGroundMaterial;

        [SerializeField]
        public Material defaultSkyboxMaterial;

        private string EnvironmentDirectory =>
            apiConfig.Environment == ApiEnvironment.Local ? devDirectory : prodDirectory;

        private string EnvironmentRoot =>
            Path.Combine(Application.persistentDataPath, EnvironmentDirectory);

        public string WorldDirectory => Path.Combine(EnvironmentRoot, worldsDirectory);
        public string ModelsDataDirectory => Path.Combine(EnvironmentRoot, modelsDataDirectory);
        public string ModelsDirectory => Path.Combine(EnvironmentRoot, modelsDirectory);
        public string SpritesDirectory => Path.Combine(EnvironmentRoot, spritesDirectory);
        public string MaterialsDirectory => Path.Combine(EnvironmentRoot, materialsDirectory);
        public string WorldFileExtension => worldsFileExtension;
        public string WorldDataFilename => worldDataFileName;
        public string CreatablesFileName => creatablesFileName;
        public string ZoneFilePrefix => zoneFilePrefix;
        public LayerMask PlaceableLayerMask => placeableLayerMask;
        public LayerMask WorldObjectLayerMask => worldObjectLayerMask;
    }
}
