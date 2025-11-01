using System.Collections.Generic;
using UnityEngine;

/*
    Manages the placement of objects in the world editor, ensuring no overlaps occur.
    Implements IDataPersistence to save and load placement data.
*/
public class PlacementManager {

    private readonly Dictionary<Vector3Int, PlacementData> occupiedCells = new();
    private readonly List<PlacementData> storedPlacedObjects = new();
    public List<PlacementData> GetAllPlacedObjects() => storedPlacedObjects;

    // Adds and returns whether placement was successful
    public (bool, PlacementData) CheckAndPlaceObject(Vector3Int gridPosition, ObjectData objectData) {
        List<Vector3Int> positions = CalculatePositions(gridPosition, objectData.Size);

        // Check for overlaps
        foreach (var pos in positions) {
            if (occupiedCells.ContainsKey(pos)) {
                Debug.LogWarning($"Cannot place object at {gridPosition}: cell {pos} is already occupied by another object.");
                return (false, null);
            }
        }

        // Store placement data with ObjectData reference
        PlacementData data = new PlacementData(gridPosition, positions, objectData);
        foreach (var pos in data.OccupiedPositions) {
            occupiedCells.Add(pos, data);
        }
        storedPlacedObjects.Add(data);
        return (true, data);
    }

    private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int size) {
        var positions = new List<Vector3Int>(size.x * size.y);

        // Calculate offsets from center
        int halfWidth = size.x / 2;
        int halfLength = size.y / 2;

        // Adjust for even-sized objects
        int xOffset = size.x % 2 == 0 ? 1 : 0;
        int zOffset = size.y % 2 == 0 ? 1 : 0;

        // Iterate from negative half to positive half
        for (int x = -halfWidth; x < halfWidth + (1 - xOffset); x++) {
            for (int z = -halfLength; z < halfLength + (1 - zOffset); z++) {
                positions.Add(new Vector3Int(
                    gridPosition.x + x,
                    gridPosition.y,
                    gridPosition.z + z
                ));
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

        storedPlacedObjects.Remove(data);
        return data.gameObject;
    }
}

