using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class PlacementData {
    [SerializeField]
    public Vector3Int Position;
    [SerializeField]
    public int AssetDataId;
    [NonSerialized]
    public List<Vector3Int> OccupiedPositions;
    [NonSerialized]
    public GameObject InstancedGameObject;

    public PlacementData(Vector3Int position, List<Vector3Int> occupiedPositions, AssetData assetData) {
        Position = position;
        OccupiedPositions = occupiedPositions;
        AssetDataId = assetData.Id;
        InstancedGameObject = assetData.AssetModelInstance;
    }
}
