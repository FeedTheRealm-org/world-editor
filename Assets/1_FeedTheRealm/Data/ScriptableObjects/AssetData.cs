using System;
using UnityEngine;
/// <summary>
/// This is the Asset Data type used to store Assets in the Database
/// Assets are considered all kind of placeable objects in the world editor
/// </summary>
[Serializable]
public class AssetData {
    [SerializeField]
    private string name;
    [SerializeField]
    private int id;
    [SerializeField]
    private Vector2Int size = Vector2Int.one;
    [SerializeField]
    private GameObject prefab;

    public string Name => name;
    public int Id => id;
    public Vector2Int Size => size;
    public GameObject Prefab => prefab;

}
