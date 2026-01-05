using System.Collections.Generic;
using Models;

public interface ICreatableLoader
{
    List<ICreatable> GetCreatables();
    void AddCreatable(ICreatable creatable);
    void RemoveCreatable(ICreatable creatable);
    void UpdateCreatable(ICreatable creatable);
}
