using Models;
using UnityEngine;

public class StructureController : MonoBehaviour, IPersistent
{
    public string id;
    public string structureName;
    public Vector3 size;
    public Vector3 rotation;
    public Vector3 offset;

    public void SaveData(ref WorldData worldData)
    {
        if (!gameObject.activeSelf)
            return;

        StructureData structureData = new(
            id,
            structureName,
            size,
            rotation,
            offset,
            transform.position
        );
        worldData.objectPlacementData.Add(structureData);
    }
}
