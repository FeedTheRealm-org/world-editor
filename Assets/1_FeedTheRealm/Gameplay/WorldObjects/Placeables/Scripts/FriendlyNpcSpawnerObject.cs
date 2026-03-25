using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldObjects
{
    public class FriendlyNpcSpawnerObject : Placeable<NPCSpawnerData>
    {
        public NPCSpawnerData data;

        public override PlaceableObjectCategories Category =>
            PlaceableObjectCategories.FriendlyNpcSpawner;

        public override void SaveData(ref ZoneData worldData)
        {
            data.Position = gameObject.transform.position;
            data.Radius = transform.localScale.x;
            worldData.npcSpawnAreas.Add(data);
        }

        public override void LoadData(NPCSpawnerData data)
        {
            this.data = data;
            gameObject.transform.position = data.Position;
            gameObject.transform.localScale = new Vector3(
                data.Radius,
                transform.localScale.y,
                data.Radius
            );
        }
    }
}
