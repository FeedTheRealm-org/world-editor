using System.Collections.Generic;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using Models;

public interface ICreatableLoader
{
    List<CreatorObject> GetCreatables();
    void AddCreatable(CreatorObject creatable);
    void RemoveCreatable(CreatorObject creatable);
    void UpdateCreatable(CreatorObject creatable);
}
