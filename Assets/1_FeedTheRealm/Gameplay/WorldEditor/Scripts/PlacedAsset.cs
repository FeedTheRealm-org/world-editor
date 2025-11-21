using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class PlacedAsset {
    [SerializeField]
    public Vector3Int Position;
    [SerializeField]
    public int AssetDataId;
    [NonSerialized]
    public List<Vector3Int> OccupiedPositions;
    [NonSerialized]
    public GameObject InstancedGameObject;

    public PlacedAsset(Vector3Int position, List<Vector3Int> occupiedPositions, Asset assetData) {
        Position = position;
        OccupiedPositions = occupiedPositions;
        AssetDataId = assetData.Id;
        InstancedGameObject = assetData.AssetModelInstance;
    }
}
