using System.Collections.Generic;
using Models;

public class Message : CreatorObject
{
    public string sender;
    public string content;
    public string dialogId;

    public Message(MessageData data, string dialogId)
        : base(data.content, data.id, "")
    {
        sender = data.sender;
        content = data.content;
        this.dialogId = dialogId;
    }

    public Message(string id, string sender, string content, string dialogId)
        : base(content, id, "")
    {
        this.sender = sender;
        this.content = content;
        this.dialogId = dialogId;
    }

    public override void DeleteObject(ref WorldData worldData)
    {
        var dialog = worldData.dialogs.Find(d => d.id == dialogId);
        if (dialog != null && dialog.messages != null)
        {
            dialog.messages.RemoveAll(m => m.id == ObjectId);
        }
    }

    public override void SaveObject(ref WorldData worldData)
    {
        MessageData messageData = new(ObjectId, sender, content);
        var dialog = worldData.dialogs.Find(d => d.id == dialogId);
        if (dialog != null)
        {
            if (dialog.messages == null)
                dialog.messages = new List<MessageData>();
            dialog.messages.Add(messageData);
        }
    }
}
