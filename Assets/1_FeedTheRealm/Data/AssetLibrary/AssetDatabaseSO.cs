using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the Asset Database, this is used for the world editor asset Library, in the future, this will be extended to be 
/// a generic library to store any kind of obect data, like assets, items, etc
/// </summary>
[CreateAssetMenu(fileName = "AssetLibrary", menuName = "Scriptable Objects/Persistence/AssetLibrary")]
public class AssetDatabaseSO : ScriptableObject {

    public List<AssetData> objectData;


    public AssetData GetAssetById(int id) {
        return objectData.Find(obj => obj.Id == id);
    }
}
