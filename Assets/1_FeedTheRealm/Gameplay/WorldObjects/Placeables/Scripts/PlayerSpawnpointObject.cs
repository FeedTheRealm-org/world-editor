using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldObjects
{
    public class PlayerSpawnpointObject : Placeable<PlayerSpawnerData>
    {
        public override PlaceableObjectCategories Category =>
            PlaceableObjectCategories.PlayerSpawnpointSpawner;

        public override void SaveData(ref ZoneData worldData)
        {
            data.Position = gameObject.transform.position;
            data.Radius = transform.localScale.x;
            worldData.playerSpawnAreas.Add(data);
        }

        public override void LoadData(PlayerSpawnerData data)
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
