using FeedTheRealm.Core.DataPersistence;
using Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.Spawners
{
    public class PlayerSpawnerController : SpawnerController, IPersistent
    {
        PlayerSpawnerData _playerSpawnData;

        public PlayerSpawnerData PlayerSpawnData
        {
            get { return _playerSpawnData; }
            set
            {
                _playerSpawnData = value;
                transform.position = _playerSpawnData.Position;
                transform.localScale = new Vector3(
                    _playerSpawnData.Radius,
                    transform.localScale.y,
                    _playerSpawnData.Radius
                );
            }
        }

        public override void SaveData(ref WorldData worldData)
        {
            if (!gameObject.activeSelf)
                return;
            PlayerSpawnerData spawnAreaData = new(transform.position, transform.localScale.x);
            worldData.playerSpawnAreas.Add(spawnAreaData);
        }
    }
}
