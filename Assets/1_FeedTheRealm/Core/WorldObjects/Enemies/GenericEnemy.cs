using System.Collections.Generic;
using Models;

public class GenericEnemy : CreatorObject
{
    public string description;
    public int healthPoints;
    public int damage;
    public int speed;
    public int range;
    public string lootTableID;

    public GenericEnemy(EnemyData enemyData)
        : base(enemyData.name, enemyData.id, enemyData.spriteFilepath)
    {
        description = enemyData.description;
        healthPoints = enemyData.healthPoints;
        damage = enemyData.damage;
        speed = enemyData.speed;
        range = enemyData.range;
        lootTableID = enemyData.lootTableID;
    }

    public override void DeleteObject(ref WorldData worldData)
    {
        worldData.enemies.RemoveAll(enemy => enemy.id == ObjectId);
    }

    public override void SaveObject(ref WorldData worldData)
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
            lootTableID
        );
        worldData.enemies.Add(enemyData);
    }
}
