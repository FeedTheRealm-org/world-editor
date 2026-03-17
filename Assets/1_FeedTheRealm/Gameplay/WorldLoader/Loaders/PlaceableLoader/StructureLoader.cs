using System.Collections.Generic;
using FeedTheRealm.Core.WorldObjects.Provider;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.Loaders
{
    public class StructureLoader : PlaceableLoader<StructureData>
    {
        protected override List<StructureData> GetData(WorldData worldData)
        {
            return worldData.objectPlacementData;
        }

        protected override GameObject GetPrefab(WorldPrefabProvider prefabProvider)
        {
            return prefabProvider.structurePrefab;
        }
    }
}
