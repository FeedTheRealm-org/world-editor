using System.Collections.Generic;
using Models;

public class Dialog : CreatorObject
{
    public string npc;

    public Dialog(DialogData dialogData)
        : base(dialogData.name, dialogData.id, "")
    {
        npc = dialogData.npc;
    }

    public override void DeleteObject(ref WorldData worldData)
    {
        worldData.dialogs.RemoveAll(d => d.id == ObjectId);
    }

    public override void SaveObject(ref WorldData worldData)
    {
        DialogData dialogData = new(ObjectId, name, npc);
        worldData.dialogs.Add(dialogData);
    }
}
