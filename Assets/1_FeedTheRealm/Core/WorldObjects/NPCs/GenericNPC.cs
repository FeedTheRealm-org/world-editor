using System.Collections.Generic;
using Models;

public class GenericNPC : CreatorObject
{
    public string description;
    public NPCDialogData npcDialog;

    public GenericNPC(NPCData npcData)
        : base(npcData.name, npcData.id, npcData.spriteFilepath)
    {
        description = npcData.description;
        npcDialog = npcData.npcDialog;
    }

    public override void DeleteObject(ref WorldData worldData)
    {
        worldData.npcs.RemoveAll(npc => npc.id == ObjectId);
    }

    public override void SaveObject(ref WorldData worldData)
    {
        NPCData npcData = new(ObjectId, DisplayName, description, spriteFile, npcDialog);
        worldData.npcs.Add(npcData);
    }
}
