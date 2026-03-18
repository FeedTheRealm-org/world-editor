using FeedTheRealm.Core.WorldObjects.Provider;
using FTRShared.Runtime.Models;
using VContainer;

namespace FTR.Core.Loaders
{
    /// <summary>
    /// Interface for objects that can be loaded with data. Used in conjunction with ILoader implementations.
    /// A usage example is a structure prefab that implements ILoadable<StructureData>,
    ///  which allows it to be initialized with the correct data when loaded by a PlaceableLoader<StructureData>.
    /// </summary>
    public interface ILoadable<T>
    {
        void Load(T data);
    }
}
