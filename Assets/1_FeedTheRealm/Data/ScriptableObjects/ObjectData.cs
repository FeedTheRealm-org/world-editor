using System;
using UnityEngine;

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
    [SerializeField]
    private int uniqueID;

    // Public accessors
    public string Name => name;
    public int Id => id;
    public Vector2Int Size => size;
    public GameObject Prefab => prefab;

    // Unique ID with setter functionality
    public int UniqueID {
        get => uniqueID;
        set => uniqueID = value;
    }
}
