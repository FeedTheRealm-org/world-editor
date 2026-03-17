using System.Collections.Generic;
using FeedTheRealm.Core.WorldObjects.Provider;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.Loaders
{
    public class FriendlyNpcSpawnerLoader : PlaceableLoader<NPCSpawnerData>
    {
        protected override List<NPCSpawnerData> GetData(WorldData worldData)
        {
            return worldData.npcSpawnAreas;
        }

        protected override GameObject GetPrefab(WorldPrefabProvider prefabProvider)
        {
            return prefabProvider.friendlyNPCSpawnerPrefab;
        }
    }
}
