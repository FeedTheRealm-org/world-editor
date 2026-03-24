using Cysharp.Threading.Tasks;
using FTRShared.Runtime.Models;

namespace FTR.Core.Loaders
{
    /// <summary>
    /// Interface for loaders that can load world data into the scene.
    /// Implementations of this interface are responsible for instantiating and initializing game objects
    /// based on the provided world data and prefabs.
    /// </summary>
    public interface ILoader
    {
        UniTask Load(ZoneData zoneData);
    }
}
