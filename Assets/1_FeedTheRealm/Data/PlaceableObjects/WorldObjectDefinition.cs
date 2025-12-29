using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public abstract class WorldObjectDefinition
{
    public string id;
    public Vector3 size = Vector3.one;
    public Vector3 rotation;
    public Vector3 offset;

    public WorldObjectDefinition(string id, Vector3 size, Vector3 rotation, Vector3 offset)
    {
        this.id = id;
        this.size = size;
        this.rotation = rotation;
        this.offset = offset;
    }

    public abstract string DisplayName { get; }

    public abstract Task<GameObject> GetObject(int layerMask);
}
