using System;
using System.Collections.Generic;
using UnityEngine;


//TODO: check this[CreateAssetMenu(fileName = "AssetDatabase", menuName = "Asset Library/Asset Database")]
[CreateAssetMenu]
public class AssetDatabaseSO : ScriptableObject {

    public List<ObjectData> objectData;
}

[Serializable]
public class ObjectData {
    [SerializeField]
    private string name;
    [SerializeField]
    private int id;
    [SerializeField]
    private Vector2Int size = Vector2Int.one;
    [SerializeField]
    private GameObject prefab;

    // Public accessors
    public string Name => name;
    public int Id => id;
    public Vector2Int Size => size;
    public GameObject Prefab => prefab;

}
