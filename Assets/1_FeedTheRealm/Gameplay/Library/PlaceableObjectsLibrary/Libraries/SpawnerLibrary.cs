using System.Collections.Generic;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Core.WorldObjects.Provider;
using VContainer;

namespace FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary
{
    public class SpawnerLibrary : PrefabLibrary
    {
        public SpawnerLibrary(WorldPrefabProvider prefabProvider, IObjectResolver resolver)
            : base(resolver)
        {
            Register(PlaceableOptions.AggresiveNPC, prefabProvider.aggresiveNPCSpawnerPrefab);
            Register(PlaceableOptions.FriendlyNPC, prefabProvider.friendlyNPCSpawnerPrefab);
            Register(PlaceableOptions.PlayerSpawnpoint, prefabProvider.playerSpawnpointPrefab);
        }

        public override List<PlaceableOption> ListAvailableItems() =>
            new()
            {
                new()
                {
                    category = PlaceableObjectCategories.Spawner,
                    id = PlaceableOptions.AggresiveNPC,
                    displayName = PlaceableOptions.AggresiveNPC,
                },
                new()
                {
                    category = PlaceableObjectCategories.Spawner,
                    id = PlaceableOptions.FriendlyNPC,
                    displayName = PlaceableOptions.FriendlyNPC,
                },
                new()
                {
                    category = PlaceableObjectCategories.Spawner,
                    id = PlaceableOptions.PlayerSpawnpoint,
                    displayName = PlaceableOptions.PlayerSpawnpoint,
                },
            };
    }
}
