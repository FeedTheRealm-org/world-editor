using System.Collections.Generic;
using FeedTheRealm.Core.Interfaces;

public interface IPlaceableLoader
{
    void LoadLibrary();
    List<IPlaceable> GetObjects();
}
