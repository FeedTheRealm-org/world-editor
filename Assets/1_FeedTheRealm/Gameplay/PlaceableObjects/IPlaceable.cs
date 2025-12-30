using System.Threading.Tasks;
using UnityEngine;

public interface IPlaceable
{
    string DisplayName { get; }
    Task<GameObject> GetPlaceableObject(int layerMask);
}
