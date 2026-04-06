using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldLoader
{
    public class FriendlyNpcSpawnerLoader : PlaceableLoader<NPCSpawnerData>
    {
        public FriendlyNpcSpawnerLoader(Logging.Logger logger, PlaceablesLibrary placeableLibrary)
            : base(logger, placeableLibrary) { }

        protected override List<NPCSpawnerData> GetData(ZoneData zoneData)
        {
            return zoneData.npcSpawnAreas;
        }

        protected override async UniTask<GameObject> GetObject(NPCSpawnerData data)
        {
            return await placeableLibrary.GetObject(
                PlaceableObjectCategories.Spawner,
                PlaceableOptions.FriendlyNPC
            );
        }
    }
}
