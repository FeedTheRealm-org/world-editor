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
    public List<EnemyLootItemData> lootItems = new List<EnemyLootItemData>();
    public float goldAmount;

    public GenericEnemy(EnemyData enemyData)
        : base(enemyData.name, enemyData.id)
    {
        description = enemyData.description;
        healthPoints = enemyData.healthPoints;
        damage = enemyData.damage;
        speed = enemyData.speed;
        canMove = enemyData.canMove;
        range = enemyData.range;
        spriteId = enemyData.spriteId;
        lootItems = enemyData.lootItems;
        goldAmount = enemyData.goldAmount;
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
            canMove,
            range,
            spriteId,
            lootItems,
            goldAmount
        );
        worldData.enemies.Add(enemyData);
    }
}
