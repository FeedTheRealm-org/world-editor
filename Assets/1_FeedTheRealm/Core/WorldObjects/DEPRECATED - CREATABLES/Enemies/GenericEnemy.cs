using System.Collections.Generic;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Core.WorldObjects.Enemies
{
    public class GenericEnemy : CreatorObject
    {
        public string description;
        public int healthPoints;
        public int damage;
        public int speed;
        public int range;
        public string lootTableId;

        public GenericEnemy(EnemyData enemyData)
            : base(enemyData.name, enemyData.id, enemyData.spriteFilePath)
        {
            description = enemyData.description;
            healthPoints = enemyData.healthPoints;
            damage = enemyData.damage;
            speed = enemyData.speed;
            range = enemyData.range;
            lootTableId = enemyData.lootTableId;
        }

        public override void DeleteObject(ref WorldDataOld worldData)
        {
            worldData.enemies.RemoveAll(enemy => enemy.id == ObjectId);
        }

        public override void SaveObject(ref WorldDataOld worldData)
        {
            EnemyData enemyData = new(
                ObjectId,
                DisplayName,
                description,
                healthPoints,
                damage,
                speed,
                range,
                spriteFile,
                lootTableId
            );
            worldData.enemies.Add(enemyData);
        }
    }
}
