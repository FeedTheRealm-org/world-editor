using System.Collections.Generic;
using FeedTheRealm.Core.Interfaces;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using Models;

namespace FeedTheRealm.Gameplay.Loaders
{
    public interface ICreatableLoader
    {
        List<CreatorObject> GetCreatables();
        void AddCreatable(CreatorObject creatable);
        void RemoveCreatable(CreatorObject creatable);
        void UpdateCreatable(CreatorObject creatable);
    }

    public interface ILoadable
    {
        void LoadWorld(WorldData worldData);
    }

    public interface IPlaceableLoader
    {
        void LoadLibrary();
        List<IPlaceable> GetObjects();
    }
}
