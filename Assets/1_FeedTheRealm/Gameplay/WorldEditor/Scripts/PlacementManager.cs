using System.Collections.Generic;
using UnityEngine;

public class PlacementManager {
    private readonly Dictionary<Vector3Int, PlacementData> occupiedCells = new();

    // Adds an object to the grid
    public (bool, GameObject) AddPlacedObject(Vector3Int gridPosition, ObjectData objectData) {
        List<Vector3Int> positions = CalculatePositions(gridPosition, objectData.Size);

        // Check for overlaps
        foreach (var pos in positions) {
            if (occupiedCells.ContainsKey(pos)) {
                Debug.LogWarning($"Cannot place object at {gridPosition}: cell {pos} is already occupied by object ID {occupiedCells[pos].ObjectData.Id}.");
                return (false, null);
            }
        }

        // Store placement data with ObjectData reference
        PlacementData data = new PlacementData(positions, objectData);
        foreach (var pos in positions) {
            occupiedCells.Add(pos, data);
        }

        return (true, data.PlacedGameObject);
    }

    private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int size) {
        var positions = new List<Vector3Int>(size.x * size.y);
        for (int x = 0; x < size.x; x++) {
            for (int z = 0; z < size.y; z++) {
                positions.Add(new Vector3Int(gridPosition.x + x, gridPosition.y, gridPosition.z + z));
            }
        }
        return positions;
    }

    public bool IsCellOccupied(Vector3Int cell) => occupiedCells.ContainsKey(cell);

    // Removes an object and returns its data (null if nothing was there)
    public GameObject RemovePlacedObject(Vector3Int gridPosition) {
        if (!occupiedCells.TryGetValue(gridPosition, out var data))
            return null;

        foreach (var pos in data.OccupiedPositions)
            occupiedCells.Remove(pos);

        return data.PlacedGameObject;
    }
}

public class PlacementData {
    public List<Vector3Int> OccupiedPositions;
    public ObjectData ObjectData;

    public GameObject PlacedGameObject;

    public PlacementData(List<Vector3Int> occupiedPositions, ObjectData objectData) {
        OccupiedPositions = occupiedPositions;
        ObjectData = objectData;
        PlacedGameObject = Object.Instantiate(objectData.Prefab);
    }
}
