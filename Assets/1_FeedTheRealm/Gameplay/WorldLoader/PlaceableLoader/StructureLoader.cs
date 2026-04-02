using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldLoader
{
    public class StructureLoader : PlaceableLoader<StructureData>
    {
        public StructureLoader(Logging.Logger logger, PlaceablesLibrary placeableLibrary)
            : base(logger, placeableLibrary) { }

        protected override List<StructureData> GetData(ZoneData zoneData)
        {
            return zoneData.objectPlacementData;
        }

        protected override async UniTask<GameObject> GetObject(StructureData data)
        {
            return await placeableLibrary.GetObject(PlaceableObjectCategories.Structure, data.id);
        }
    }
}
