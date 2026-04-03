namespace FeedTheRealm.Core.Library
{
    /// <summary>
    /// Categories for placeable objects in the world editor.
    /// These are used to determine which editor prefab to use when editing a placeable object.
    /// For placing, we use the 'Spawner' type with a specific SpawnerType to determine which object to place,
    /// while for editing we use the 'PlaceableObjectCategories' to determine which editor to use.
    /// </summary>
    public enum PlaceableObjectCategories
    {
        // Generic categories
        Structure,
        Spawner,
        Misc,

        // Specific categories
        FriendlyNpcSpawner,
        AggresiveNpcSpawner,
        PlayerSpawnpointSpawner,
        Portal,
    }
}
