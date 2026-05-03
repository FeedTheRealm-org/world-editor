using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldLoader
{
    public class ChestLoader : PlaceableLoader<ChestData>
    {
        public ChestLoader(Logging.Logger logger, PlaceablesLibrary placeableLibrary)
            : base(logger, placeableLibrary) { }

        protected override List<ChestData> GetData(ZoneData zoneData)
        {
            return zoneData.chestPlacements;
        }

        protected override async UniTask<GameObject> GetObject(ChestData data)
        {
            return await placeableLibrary.GetObject(
                PlaceableObjectCategories.Misc,
                PlaceableOptions.Chest
            );
        }
    }
}
