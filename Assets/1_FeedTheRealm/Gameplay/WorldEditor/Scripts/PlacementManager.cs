using System.Collections.Generic;
using UnityEngine;

// TODO: refactor this to be a monobehavior script insted of a regular class
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
