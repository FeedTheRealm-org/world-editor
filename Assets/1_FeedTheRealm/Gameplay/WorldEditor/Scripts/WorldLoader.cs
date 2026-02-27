using Models;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Gameplay.WorldEditor
{
    public class WorldLoader
    {
        [Inject]
        private readonly DataPersistenceManagerSO dataPersistenceManager;

        public WorldLoader()
        {
            Debug.Log("WorldLoader constructed");
        }

        public void Load()
        {
            Debug.Log("WorldLoader.Load() called");
            WorldData worldData = dataPersistenceManager.CurrentWorldData;
            LoadWorld(worldData);
        }

        // TODO: consider adding a loading screen or something to avoid having the user
        //see how the world is being populated.
        public void LoadWorld(WorldData worldData)
        {
            Debug.Log("Raising world selected event...");
            //SelectionRaiser.RaiseSelected(worldData);
        }
    }
}


// public class WorldLoader : MonoBehaviour
// {
//     [SerializeField]
//     private Logging.Logger logger;

//     [SerializeField]
//     private DataPersistenceManagerSO dataPersistenceManager;

//     [SerializeField]
//     private CreatorObjectLibrarySO creatorObjectLibrary;

//     void Awake()
//     {
//         InitializeLibraries();
//         WorldData worldData = dataPersistenceManager.CurrentWorldData;
//         LoadWorld(worldData);
//     }

//     // TODO: This is to force initialization of the libraries.
//     // Find a better way to do this.
//     private void InitializeLibraries()
//     {
//         creatorObjectLibrary.Initialize();
//     }

//     // TODO: consider adding a loading screen or something to avoid having the user
//     // see how the world is being populated.
//     public void LoadWorld(WorldData worldData)
//     {
//         logger.Log("Raising world selected event...", this, Logging.LogType.Info);
//         SelectionRaiser.RaiseSelected(worldData);
//     }
// }
