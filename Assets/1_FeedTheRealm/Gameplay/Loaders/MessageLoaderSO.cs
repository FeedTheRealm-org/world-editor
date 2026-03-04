using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FeedTheRealm.Core.WorldObjects.Dialogs;
using Models;
using UnityEngine;

[CreateAssetMenu(fileName = "MessageLoader", menuName = "Scriptable Objects/Loaders/MessageLoader")]
public class MessageLoaderSO : ScriptableObject, ILoadable, ICreatableLoader
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private WorldSelectedEvent worldSelectedEvent;

    private List<CreatorObject> messages = new();

    void OnEnable()
    {
        worldSelectedEvent.OnRaised += LoadWorld;
    }

    void OnDisable()
    {
        worldSelectedEvent.OnRaised -= LoadWorld;
    }

    public List<CreatorObject> GetCreatables()
    {
        return messages.FindAll(item => !item.IsDeleted);
    }

    public void AddCreatable(CreatorObject creatable)
    {
        messages.Add(creatable);
    }

    public void RemoveCreatable(CreatorObject creatable)
    {
        creatable.Delete();
        messages.Remove(creatable);
    }

    public void UpdateCreatable(CreatorObject creatable)
    {
        int index = messages.FindIndex(item => item.ObjectId == creatable.ObjectId);
        if (index != -1)
        {
            messages[index] = creatable;
        }
    }

    public void LoadWorld(WorldData worldData)
    {
        messages.Clear();
        if (worldData == null)
        {
            logger.Log(
                "MessageLoader.LoadWorld: worldData is null.",
                this,
                Logging.LogType.Warning
            );
            return;
        }

        foreach (var dialog in worldData.dialogs ?? new List<DialogData>())
        {
            foreach (MessageData messageData in dialog.messages ?? new List<MessageData>())
            {
                messages.Add(new Message(messageData, dialog.id));
            }
        }
    }
}
