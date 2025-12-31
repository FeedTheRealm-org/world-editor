using System.Collections.Generic;
using Models;
using Utils;

public interface ILoadable
{
    void LoadLibrary();
    List<IPlaceable> GetObjects();
    void LoadWorld(WorldData worldData);
}
