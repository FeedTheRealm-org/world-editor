using Models;
using UnityEngine;

public class StructureController : MonoBehaviour, IPersistent
{
    public string id;
    public string objectName;
    public Vector3 size;
    public Vector3 rotation;
    public Vector3 offset;

    public void SaveData(ref WorldData worldData)
    {
        StructureData structureData = new(
            id,
            objectName,
            size,
            rotation,
            offset,
            transform.position
        );
        worldData.objectPlacementData.Add(structureData);
    }
}
