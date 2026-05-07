using System.IO;
using UnityEngine;

namespace FTR.Core.Common.Config
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Config/Config")]
    public class Config : ScriptableObject
    {
        [Header("StreamingAssets directories")]
        [SerializeField]
        private string modelsDataDirectory = "Models";

        [SerializeField]
        private string spritesDirectory = "Sprites";

        [SerializeField]
        private string zoneGroundDirectory = "ZoneMaterials/Floor";

        [SerializeField]
        private string zoneSkyboxDirectory = "ZoneMaterials/Skybox";

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

        [Header("Default Zone Material")]
        public Material defaultZoneMaterial;
        public string defaultMaterialId = "DefaultZoneTexture";

        public string WorldDirectory =>
            Path.Combine(Application.persistentDataPath, worldsDirectory);
        public string ModelsDataDirectory =>
            Path.Combine(Application.persistentDataPath, modelsDataDirectory);
        public string ModelsDirectory =>
            Path.Combine(Application.streamingAssetsPath, modelsDirectory);
        public string SpritesDirectory =>
            Path.Combine(Application.streamingAssetsPath, spritesDirectory);
        public string ZoneGroundDirectory =>
            Path.Combine(Application.streamingAssetsPath, zoneGroundDirectory);
        public string ZoneSkyboxDirectory =>
            Path.Combine(Application.streamingAssetsPath, zoneSkyboxDirectory);
        public string WorldFileExtension => worldsFileExtension;
        public string WorldDataFilename => worldDataFileName;
        public string CreatablesFileName => creatablesFileName;
        public string ZoneFilePrefix => zoneFilePrefix;
        public LayerMask PlaceableLayerMask => placeableLayerMask;
        public LayerMask WorldObjectLayerMask => worldObjectLayerMask;
    }
}
