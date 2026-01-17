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
    public LootTableData lootTable;

    public GenericEnemy(EnemyData enemyData)
        : base(enemyData.name, enemyData.id, enemyData.spriteFilepath)
    {
        description = enemyData.description;
        healthPoints = enemyData.healthPoints;
        damage = enemyData.damage;
        speed = enemyData.speed;
        range = enemyData.range;
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
            spriteFile,
            lootTable
        );
        worldData.enemies.Add(enemyData);
    }
}
