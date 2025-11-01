using System;
using System.Collections.Generic;
using UnityEngine;

// TODO: review to simplify this data
// This can be simplified, but for now, it holds necessary placement info


//TODO: ONLY SAVE THE OBJECT REF ID, NOT THE FULL OBJECTDATA
[Serializable]
public class PlacementData {
    [SerializeField]
    public Vector3Int GridPosition;
    [SerializeField]
    public List<Vector3Int> OccupiedPositions;
    [SerializeField]
    public ObjectData ObjectDataRef;

    public GameObject gameObject = null;
    public PlacementData(Vector3Int gridPosition, List<Vector3Int> occupiedPositions, ObjectData objectData) {
        GridPosition = gridPosition;
        OccupiedPositions = occupiedPositions;
        ObjectDataRef = objectData;
        gameObject = UnityEngine.Object.Instantiate(ObjectDataRef.Prefab);
    }
}