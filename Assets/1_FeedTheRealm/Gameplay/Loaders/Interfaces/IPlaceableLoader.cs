using System.Collections.Generic;

public interface IPlaceableLoader
{
    void LoadLibrary();
    List<IPlaceable> GetObjects();
}
