using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface ILoadable
{
    void LoadLibrary();
    List<WorldObjectDefinition> GetObjects();
}
