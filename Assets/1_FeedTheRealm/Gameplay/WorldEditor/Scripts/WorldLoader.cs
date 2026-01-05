using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;
using Utils;

public class WorldLoader : MonoBehaviour
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private DataPersistenceManagerSO dataPersistenceManager;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    void Awake()
    {
        InitializeLibraries();
        WorldData worldData = dataPersistenceManager.CurrentWorldData;
        LoadWorld(worldData);
    }

    // TODO: This is to force initialization of the libraries.
    // Find a better way to do this.
    private void InitializeLibraries()
    {
        creatorObjectLibrary.Initialize();
    }

    // TODO: consider adding a loading screen or something to avoid having the user
    // see how the world is being populated.
    public void LoadWorld(WorldData worldData)
    {
        logger.Log("Raising world selected event...", this, Logging.LogType.Info);
        SelectionRaiser.RaiseSelected(worldData);
    }
}
