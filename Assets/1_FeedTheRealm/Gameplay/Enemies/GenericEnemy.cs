using System.Collections.Generic;
using Models;

public class GenericEnemy : CreatorObject
{
    public string description;
    public int healthPoints;
    public int damage;
    public int speed;
    public bool canMove;
    public int range;
    public string spriteId;
    public LootTableData lootTable;

    public GenericEnemy(EnemyData enemyData)
        : base(enemyData.name, enemyData.id)
    {
        description = enemyData.description;
        healthPoints = enemyData.healthPoints;
        damage = enemyData.damage;
        speed = enemyData.speed;
        range = enemyData.range;
        spriteId = enemyData.spriteId;
        lootTable = enemyData.lootTable;
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
            spriteId,
            lootTable
        );
        worldData.enemies.Add(enemyData);
    }
}
