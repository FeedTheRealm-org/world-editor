using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldLoader
{
    public class PortalLoader : PlaceableLoader<PortalPlacementData>
    {
        public PortalLoader(Logging.Logger logger, PlaceablesLibrary placeableLibrary)
            : base(logger, placeableLibrary) { }

        protected override List<PortalPlacementData> GetData(ZoneData zoneData)
        {
            return zoneData.portalPlacements;
        }

        protected override async UniTask<GameObject> GetObject(PortalPlacementData data)
        {
            return await placeableLibrary.GetObject(
                PlaceableObjectCategories.Misc,
                PlaceableOptions.Portal
            );
        }
    }
}
