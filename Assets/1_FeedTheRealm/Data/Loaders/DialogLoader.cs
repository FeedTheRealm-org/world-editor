using System.Collections.Generic;
using Models;
using UnityEngine;
using Utils;

[CreateAssetMenu(fileName = "DialogLoader", menuName = "Scriptable Objects/Loaders/DialogLoader")]
public class DialogLoader : ScriptableObject, ILoadable, ICreatableLoader
{
    [SerializeField]
    private Logging.Logger logger;

    private List<CreatorObject> dialogs = new();

    void OnEnable()
    {
        SelectionRaiser.WorldSelected += LoadWorld;
    }

    void OnDisable()
    {
        SelectionRaiser.WorldSelected -= LoadWorld;
    }

    public List<CreatorObject> GetCreatables()
    {
        return dialogs.FindAll(item => !item.IsDeleted);
    }

    public void AddCreatable(CreatorObject creatable)
    {
        dialogs.Add(creatable);
    }

    public void RemoveCreatable(CreatorObject creatable)
    {
        creatable.Delete();
        dialogs.Remove(creatable);
    }

    public void UpdateCreatable(CreatorObject creatable)
    {
        int index = dialogs.FindIndex(item => item.ObjectId == creatable.ObjectId);
        if (index != -1)
        {
            dialogs[index] = creatable;
        }
    }

    public void LoadWorld(WorldData worldData)
    {
        dialogs.Clear();
        if (worldData == null)
        {
            logger.Log("DialogLoader.LoadWorld: worldData is null.", this, Logging.LogType.Warning);
            return;
        }

        foreach (DialogData itemData in worldData.dialogs ?? new List<DialogData>())
        {
            dialogs.Add(new Dialog(itemData));
        }
    }
}
