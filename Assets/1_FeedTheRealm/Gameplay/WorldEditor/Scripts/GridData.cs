using System.Collections.Generic;
using UnityEngine;

public class GridData {
    private readonly Dictionary<Vector3Int, PlacementData> occupiedCells = new();

    public bool AddPlacedObject(Vector3Int gridPosition, Vector2Int size, int placedObjectIndex, int objectID) {
        List<Vector3Int> positions = CalculatePositions(gridPosition, size);

        // Check for overlaps directly in the dictionary
        foreach (var pos in positions) {
            if (occupiedCells.ContainsKey(pos)) {
                Debug.LogWarning($"Cannot place object at {gridPosition}: cell {pos} is already occupied by object ID {occupiedCells[pos].ObjectID}.");
                return false;
            }
        }

        // Mark all positions as occupied
        PlacementData data = new PlacementData(positions, placedObjectIndex, objectID);
        foreach (var pos in positions) {
            occupiedCells.Add(pos, data);
        }

        return true;
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

    public void RemovePlacedObject(Vector3Int gridPosition) {
        if (!occupiedCells.TryGetValue(gridPosition, out var data)) return;

        foreach (var pos in data.occupiedPositions)
            occupiedCells.Remove(pos);
    }
}

public class PlacementData {
    public List<Vector3Int> occupiedPositions;
    public int PlacedObjectIndex;
    public int ObjectID;

    public PlacementData(List<Vector3Int> occupiedPositions, int placedObjectIndex, int objectID) {
        this.occupiedPositions = occupiedPositions;
        PlacedObjectIndex = placedObjectIndex;
        ObjectID = objectID;
    }
}
