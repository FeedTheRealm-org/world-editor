using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AssetLibraryV2", menuName = "Scriptable Objects/Persistence/AssetLibraryV2")]
public class AssetLibraryV2SO : InitializableSO {
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Asset storage config")]
    [SerializeField] public string assetsFilePath = "Assets/models.json";
    [SerializeField] private Logging.Logger logger;


    private List<GameObject> worldObjects = new();

    protected override void OnInitialize() {

    }

    protected override void OnReset() {

    }
}
