using System.Threading.Tasks;
using FeedTheRealm.Core.Interfaces;
using UnityEngine;

namespace FeedTheRealm.Core.Interfaces
{
    public interface IPlaceable
    {
        string DisplayName { get; }
        Task<GameObject> GetPlaceableObject(int layerMask);
    }
}
