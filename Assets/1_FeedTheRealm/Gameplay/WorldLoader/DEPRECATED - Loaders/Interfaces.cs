using System.Collections.Generic;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.WorldLoader
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
        void LoadWorld(WorldDataOld worldData);
    }
}
