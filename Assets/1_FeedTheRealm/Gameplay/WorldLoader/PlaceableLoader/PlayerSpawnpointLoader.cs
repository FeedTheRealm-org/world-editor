using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldLoader
{
    public class PlayerSpawnpointLoader : PlaceableLoader<PlayerSpawnerData>
    {
        public PlayerSpawnpointLoader(Logging.Logger logger, PlaceablesLibrary placeableLibrary)
            : base(logger, placeableLibrary) { }

        protected override List<PlayerSpawnerData> GetData(ZoneData zoneData)
        {
            return zoneData.playerSpawnAreas;
        }

        protected override async UniTask<GameObject> GetObject(PlayerSpawnerData data)
        {
            return await placeableLibrary.GetObject(
                PlaceableObjectCategories.Spawner,
                PlaceableOptions.PlayerSpawnpoint
            );
        }
    }
}
