using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldObjects
{
    public class FriendlyNpcSpawnerObject : Placeable<NPCSpawnerData>
    {
        public override PlaceableObjectCategories Category =>
            PlaceableObjectCategories.FriendlyNpcSpawner;

        public override void SaveData(ref ZoneData worldData)
        {
            data.Position = gameObject.transform.position;
            data.Radius = transform.localScale.x / 2f;
            worldData.npcSpawnAreas.Add(data);
        }

        public override void LoadData(NPCSpawnerData data)
        {
            this.data = data;
            gameObject.transform.position = data.Position;
            gameObject.transform.localScale = new Vector3(
                data.Radius * 2f,
                transform.localScale.y,
                data.Radius * 2f
            );
        }
    }
}
