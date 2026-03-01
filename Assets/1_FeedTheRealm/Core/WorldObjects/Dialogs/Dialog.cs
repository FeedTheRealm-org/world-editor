using Models;

public class Dialog : CreatorObject
{
    public Dialog(DialogData dialogData)
        : base(dialogData.name, dialogData.id, "") { }

    public override void DeleteObject(ref WorldData worldData)
    {
        worldData.dialogs.RemoveAll(d => d.id == ObjectId);
    }

    public override void SaveObject(ref WorldData worldData)
    {
        DialogData dialogData = new(ObjectId, name);
        worldData.dialogs.Add(dialogData);
    }
}
