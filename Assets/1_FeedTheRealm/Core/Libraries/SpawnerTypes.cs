using System.Collections.Generic;

namespace FeedTheRealm.Core.Library
{
    /// <summary>
    /// These are diferent from PlaceableObjectCategories because they are used to identify the type of spawner,
    /// while PlaceableObjectCategories is used to identify the category of placeable object.
    /// </summary>
    public static class SpawnerTypes
    {
        public const string AggresiveNPC = "Aggresive NPC";
        public const string FriendlyNPC = "Friendly NPC";
        public const string PlayerSpawnpoint = "Player Spawnpoint";
    }
}
