using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldLoader
{
    public class AggresiveNpcSpawnerLoader : PlaceableLoader<EnemySpawnerData>
    {
        public AggresiveNpcSpawnerLoader(Logging.Logger logger, PlaceablesLibrary placeableLibrary)
            : base(logger, placeableLibrary) { }

        protected override List<EnemySpawnerData> GetData(ZoneData zoneData)
        {
            return zoneData.enemySpawnAreas;
        }

        protected override async UniTask<GameObject> GetObject(EnemySpawnerData data)
        {
            return await placeableLibrary.GetObject(
                PlaceableObjectCategories.Spawner,
                PlaceableOptions.AggresiveNPC
            );
        }
    }
}
