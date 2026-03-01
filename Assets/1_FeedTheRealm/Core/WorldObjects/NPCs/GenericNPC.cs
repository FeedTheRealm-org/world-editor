using System.Collections.Generic;
using Models;

public class GenericNPC : CreatorObject
{
    public string description;

    public GenericNPC(NPCData npcData)
        : base(npcData.name, npcData.id, npcData.spriteFilePath)
    {
        description = npcData.description;
    }

    public override void DeleteObject(ref WorldData worldData)
    {
        worldData.npcs.RemoveAll(npc => npc.id == ObjectId);
    }

    public override void SaveObject(ref WorldData worldData)
    {
        NPCData npcData = new(ObjectId, DisplayName, description, spriteFile);
        worldData.npcs.Add(npcData);
    }
}
