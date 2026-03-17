using System.Collections.Generic;
using FeedTheRealm.Core.WorldObjects.Provider;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.Loaders
{
    public class PlayerSpawnpointLoader : PlaceableLoader<PlayerSpawnerData>
    {
        protected override List<PlayerSpawnerData> GetData(WorldData worldData)
        {
            return worldData.playerSpawnAreas;
        }

        protected override GameObject GetPrefab(WorldPrefabProvider prefabProvider)
        {
            return prefabProvider.playerSpawnerPrefab;
        }
    }
}
