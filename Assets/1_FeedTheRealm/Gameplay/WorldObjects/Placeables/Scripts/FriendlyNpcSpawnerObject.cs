using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldObjects
{
    public class FriendlyNpcSpawnerObject : WorldObjectController, ILoadable<NPCSpawnerData>
    {
        public NPCSpawnerData data;

        public override PlaceableObjectCategories Category =>
            PlaceableObjectCategories.FriendlyNpcSpawner;

        public void Load(NPCSpawnerData data)
        {
            this.data = data;
            Setup();
        }

        private void Setup()
        {
            gameObject.transform.position = data.Position;
            gameObject.transform.localScale = new Vector3(
                data.Radius,
                transform.localScale.y,
                data.Radius
            );
        }

        public override void SaveData(ref WorldDataOld worldData)
        {
            data.Position = gameObject.transform.position;
            data.Radius = transform.localScale.x;
            worldData.npcSpawnAreas.Add(data);
        }
    }
}
