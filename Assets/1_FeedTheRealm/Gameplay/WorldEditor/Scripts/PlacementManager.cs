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

    public PlacementData TryPlaceObject(Asset assetData, Vector3Int gridPosition) {
        List<Vector3Int> positions = CalculatePositions(assetData, gridPosition);

        // Check for overlaps
        foreach (var pos in positions) {
            if (occupiedCells.ContainsKey(pos)) {
                return null;
            }
        }

        PlacementData data = new(gridPosition, positions, assetData);
        foreach (var pos in data.OccupiedPositions) {
            occupiedCells.Add(pos, data);
        }
        storedPlacedObjects.Add(data);
        return data;
    }

    private List<Vector3Int> CalculatePositions(Asset assetData, Vector3Int gridPosition) {
        Vector2Int size = assetData.Size;
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


    public GameObject RemovePlacedObject(Vector3Int gridPosition) {
        if (!occupiedCells.TryGetValue(gridPosition, out var placedObject))
            return null;

        foreach (var pos in placedObject.OccupiedPositions)
            occupiedCells.Remove(pos);

        storedPlacedObjects.Remove(placedObject);
        return placedObject.InstancedGameObject;
    }
}

