using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Interfaces;
using FeedTheRealm.Core.WorldObjects.PlaceableObjects;
using FeedTheRealm.Gameplay.Structures;
using FeedTheRealm.Gameplay.WorldEditor.WorldEditorStateMachine;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldLoader
{
    [Serializable]
    public class WorldObjectReferenceList
    {
        public List<StructureData> structureData;
    }

    [CreateAssetMenu(
        fileName = "StructureLoader",
        menuName = "Scriptable Objects/Loaders/StructureLoader"
    )]
    public class StructureLoaderSO : ScriptableObject, ILoadable, IPlaceableLoader
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private WorldSelectedEvent worldSelectedEvent;

        [SerializeField]
        private string libraryFilePath = "Models/models.json";

        [SerializeField]
        private string modelsDirectory = "Models";

        [SerializeField]
        private GameObject structurePrefab;
        private List<StructureObject> structureObjects = new();

        void OnEnable()
        {
            worldSelectedEvent.OnRaised += LoadWorld;
        }

        void OnDisable()
        {
            worldSelectedEvent.OnRaised -= LoadWorld;
        }

        public void LoadLibrary()
        {
            logger.Log("Loading structure library...", this, Logging.LogType.Info);
            structureObjects.Clear();
            if (!System.IO.File.Exists(PersistentLibraryFilePath))
                GenerateLibrary(PersistentLibraryFilePath);
            LoadStructureLibrary();

            logger.Log(
                "Structure library loaded. Count: " + structureObjects.Count,
                this,
                Logging.LogType.Info
            );
        }

        public List<IPlaceable> GetObjects()
        {
            return structureObjects.Cast<IPlaceable>().ToList();
        }

        public bool IsModelPresent(string structureName)
        {
            string modelPath = GetModelFilePath(structureName);
            return System.IO.File.Exists(modelPath);
        }

        public string GetModelFilePath(string structureName)
        {
            return System.IO.Path.Combine(PersistentModelsDirectory, structureName + ".glb");
        }

        public async void LoadWorld(WorldData worldData)
        {
            if (worldData.objectPlacementData == null)
                return;
            LoadLibrary(); // we make sure the library is loaded before placing objects
            try
            {
                await OnLoadAsync(worldData.objectPlacementData);
            }
            catch (Exception e)
            {
                logger.Log($"Error placing structures: {e.Message}", this, Logging.LogType.Error);
            }
        }

        // -------------------- Private Methods --------------------

        #region Path Helpers

        private string PersistentLibraryFilePath =>
            System.IO.Path.Combine(Application.persistentDataPath, libraryFilePath);

        private string PersistentModelsDirectory =>
            System.IO.Path.Combine(Application.streamingAssetsPath, modelsDirectory);

        private bool EnsurePathExists(string path, bool isDirectory = false)
        {
            if (isDirectory)
            {
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                    return false;
                }
                return true;
            }
            if (!System.IO.File.Exists(path))
            {
                string outputDirectory = System.IO.Path.GetDirectoryName(path);
                if (!System.IO.Directory.Exists(outputDirectory))
                {
                    System.IO.Directory.CreateDirectory(outputDirectory);
                }
                System.IO.File.WriteAllText(path, "");
                return false;
            }
            return true;
        }

        #endregion

        #region Library Generation

        private void LoadStructureLibrary()
        {
            logger.Log("Loading structure library...", this, Logging.LogType.Info);
            string json = System.IO.File.ReadAllText(PersistentLibraryFilePath);
            List<StructureData> structureDataList = JsonUtility
                .FromJson<WorldObjectReferenceList>(json)
                .structureData;
            foreach (var structureData in structureDataList)
            {
                StructureObject structureObject = new(
                    structureData,
                    structureData.structureName,
                    structurePrefab
                );
                structureObjects.Add(structureObject);
            }
        }

        private void GenerateLibrary(string outputPath)
        {
            logger.Log("Generating structure library...", this, Logging.LogType.Info);
            string modelsPath = PersistentModelsDirectory;
            if (!EnsurePathExists(modelsPath, true))
            {
                logger.Log(
                    "Models directory not found. Created empty directory at: "
                        + modelsPath
                        + " Please add model files here and regenerate the library.",
                    this,
                    Logging.LogType.Warning
                );
                return;
            }
            string[] objectFiles = System
                .IO.Directory.GetFiles(modelsPath, "*.*")
                .Where(f => !f.EndsWith(".meta"))
                .ToArray();

            List<StructureData> structureDataList = objectFiles
                .Select(objectFile => new StructureData(
                    Guid.NewGuid().ToString(),
                    System.IO.Path.GetFileNameWithoutExtension(objectFile),
                    Vector3.one,
                    Vector3.zero,
                    Vector3.zero,
                    Vector3.zero
                ))
                .ToList();
            string json = JsonUtility.ToJson(
                new WorldObjectReferenceList { structureData = structureDataList },
                true
            );
            string outputDirectory = System.IO.Path.GetDirectoryName(outputPath);
            if (!System.IO.Directory.Exists(outputDirectory))
            {
                System.IO.Directory.CreateDirectory(outputDirectory);
            }
            System.IO.File.WriteAllText(outputPath, json);
        }

        private async Task OnLoadAsync(List<StructureData> structureDatas)
        {
            foreach (var structureData in structureDatas)
            {
                StructureObject structureObject = structureObjects.Find(obj =>
                    obj.data.id == structureData.id
                );
                if (structureObject == null)
                    continue;
                GameObject structureInstance = await structureObject.GetPlaceableObject(
                    WorldLayers.WorldObjectLayer
                );
                structureInstance.transform.position = structureData.position;
                structureInstance.transform.localScale = structureData.size;
                structureInstance.transform.localEulerAngles = structureData.rotation;

                StructureController controller =
                    structureInstance.GetComponent<StructureController>();
                controller.isShop = structureData.isShop;
                controller.shopId = structureData.shopId;
            }
        }
    }

        #endregion
}
