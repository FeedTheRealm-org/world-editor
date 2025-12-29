using System.Collections.Generic;
using UnityEngine;

// TODO: refactor this to be a ScriptableObject-based library
public class CreatorLibraryController : MonoBehaviour
{
    public StructureLoader structureLoader = new();

    public void Initialize()
    {
        structureLoader.LoadLibrary();
        Debug.Log("Library loaded");
    }

    public List<WorldObjectDefinition> GetObjects(WorldObjectCategories category)
    {
        switch (category)
        {
            case WorldObjectCategories.Structure:
                Debug.Log("Retrieving structure objects");
                return structureLoader.GetObjects();
            default:
                return new List<WorldObjectDefinition>();
        }
    }
}
