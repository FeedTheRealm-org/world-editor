using System.Collections.Generic;
using Models;

public interface ILoadable
{
    void LoadLibrary();
    List<IPlaceable> GetObjects();
    static void LoadWorld(WorldData worldData)
    {
        throw new System.NotImplementedException();
    }
}
