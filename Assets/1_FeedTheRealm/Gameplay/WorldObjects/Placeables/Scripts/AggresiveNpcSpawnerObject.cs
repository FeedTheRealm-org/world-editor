using FeedTheRealm.Core.WorldObjects;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldObjects
{
    public class AggresiveNpcSpawnerObject : WorldObjectController, ILoadable<EnemySpawnerData>
    {
        public EnemySpawnerData data;

        public void Load(EnemySpawnerData data)
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

        public override void SaveData(ref WorldData worldData)
        {
            data.Position = gameObject.transform.position;
            data.Radius = transform.localScale.x;
            worldData.enemySpawnAreas.Add(data);
        }
    }
}
