using System.Collections.Generic;

public interface ILoadable
{
    void LoadLibrary();
    List<IPlaceable> GetObjects();
}
