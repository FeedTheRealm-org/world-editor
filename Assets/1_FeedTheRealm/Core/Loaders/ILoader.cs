using FeedTheRealm.Core.WorldObjects.Provider;
using FTRShared.Runtime.Models;
using VContainer;

namespace FTR.Core.Common.Loaders
{
    public interface ILoader
    {
        void Load(
            WorldData worldData,
            WorldPrefabProvider worldPrefabProvider,
            IObjectResolver objectResolver
        );
    }
}
