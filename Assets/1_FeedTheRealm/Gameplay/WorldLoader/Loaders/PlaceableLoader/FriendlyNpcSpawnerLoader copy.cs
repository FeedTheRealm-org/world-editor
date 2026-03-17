using System.Collections.Generic;
using FeedTheRealm.Core.WorldObjects.Provider;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.Loaders
{
    public class AggresiveNpcSpawnerLoader : PlaceableLoader<EnemySpawnerData>
    {
        protected override List<EnemySpawnerData> GetData(WorldData worldData)
        {
            return worldData.enemySpawnAreas;
        }

        protected override GameObject GetPrefab(WorldPrefabProvider prefabProvider)
        {
            return prefabProvider.aggresiveNPCSpawnerPrefab;
        }
    }
}
