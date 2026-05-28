namespace FeedTheRealm.Core.Library
{
    /// <summary>
    /// These are diferent from PlaceableObjectCategories because they are used to identify the type of spawner,
    /// while PlaceableObjectCategories is used to identify the category of placeable object.
    /// </summary>
    public static class PlaceableOptions
    {
        public const string AggresiveNPC = "Enemy";
        public const string FriendlyNPC = "Passive NPC";
        public const string PlayerSpawnpoint = "Player Spawnpoint";
        public const string Portal = "Portal";
        public const string Chest = "Chest";
    }
}
