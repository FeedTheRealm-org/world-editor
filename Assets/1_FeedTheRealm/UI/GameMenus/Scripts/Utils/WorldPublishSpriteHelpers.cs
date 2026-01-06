using System.IO;
using System.Threading.Tasks;
using API;
using Models;
using UnityEngine;

public static class WorldPublishSpriteHelpers
{
    /// <summary>
    /// Upload item sprites and normalize spriteId to use the backend ID.
    /// Also updates all references in loot tables (world and enemies).
    /// </summary>
    public static async Task UploadItemSpritesAndUpdateIds(
        WorldData worldData,
        ItemSpritesService itemsService,
        Logging.Logger logger,
        MonoBehaviour context
    )
    {
        if (worldData == null || worldData.consumableItems == null || itemsService == null)
            return;

        var itemIdToBackendSpriteId = new System.Collections.Generic.Dictionary<string, string>();

        // 1) Upload sprites for all consumable items and store backend sprite IDs by item ID.
        foreach (var item in worldData.consumableItems)
        {
            if (item == null || string.IsNullOrEmpty(item.spriteId))
                continue;

            logger.Log(
                $"Uploading sprite for consumable item '{item.name}' (pre-publish spriteId='{item.spriteId}')",
                context,
                Logging.LogType.Info
            );

            string path = Path.Combine(
                Application.streamingAssetsPath,
                "Items",
                item.spriteId + ".png"
            );

            if (!File.Exists(path))
            {
                logger.Log(
                    $"Sprite file not found for item '{item.name}' at path '{path}'. Cancelling publish.",
                    context,
                    Logging.LogType.Error
                );
                throw new System.Exception(
                    $"Sprite file not found for item '{item.name}' (spriteId='{item.spriteId}') at path '{path}'."
                );
            }

            byte[] spriteBytes = File.ReadAllBytes(path);
            if (spriteBytes == null || spriteBytes.Length == 0)
            {
                logger.Log(
                    $"Sprite bytes for asset ID '{item.spriteId}' are null or empty. Cancelling publish.",
                    context,
                    Logging.LogType.Error
                );
                throw new System.Exception(
                    $"Failed to load sprite bytes for item '{item.name}' (spriteId='{item.spriteId}')."
                );
            }

            string type = Path.GetExtension(path).Replace(".", "").ToLower();
            var (createdSprite, itemError) = await itemsService.UploadItemSprite(
                spriteBytes,
                $"{item.name}{Path.GetExtension(path)}",
                $"image/{type}"
            );

            if (!string.IsNullOrEmpty(itemError) || createdSprite == null)
            {
                logger.Log(
                    $"Failed to upload sprite for item '{item.name}': {itemError}",
                    context,
                    Logging.LogType.Error
                );
                throw new System.Exception(
                    $"Failed to upload sprite for item '{item.name}': {itemError}"
                );
            }

            logger.Log(
                $"Item '{item.name}' sprite uploaded. Backend spriteId='{createdSprite.id}', url='{createdSprite.url}'",
                context,
                Logging.LogType.Info
            );

            item.spriteId = createdSprite.id;
            if (!string.IsNullOrEmpty(item.id))
            {
                itemIdToBackendSpriteId[item.id] = createdSprite.id;
            }
        }

        // 2) Propagate spriteId to all loot tables stored globally on the world.
        if (worldData.lootTables != null && worldData.lootTables.Count > 0)
        {
            foreach (var lootTable in worldData.lootTables)
            {
                if (lootTable?.lootItems == null)
                    continue;

                foreach (var lootItem in lootTable.lootItems)
                {
                    if (lootItem == null || string.IsNullOrEmpty(lootItem.id))
                        continue;

                    if (itemIdToBackendSpriteId.TryGetValue(lootItem.id, out var backendId))
                    {
                        logger.Log(
                            $"Updating loot table '{lootTable.name}' item '{lootItem.name}' spriteId to backend spriteId='{backendId}'",
                            context,
                            Logging.LogType.Info
                        );
                        lootItem.spriteId = backendId;
                    }
                }
            }
        }

        // 3) Propagate spriteId to each enemy's loot table items.
        if (worldData.enemies != null && worldData.enemies.Count > 0)
        {
            foreach (var enemy in worldData.enemies)
            {
                var enemyLootTable = enemy?.lootTable;
                if (enemyLootTable?.lootItems == null)
                    continue;

                foreach (var lootItem in enemyLootTable.lootItems)
                {
                    if (lootItem == null || string.IsNullOrEmpty(lootItem.id))
                        continue;

                    if (itemIdToBackendSpriteId.TryGetValue(lootItem.id, out var backendId))
                    {
                        logger.Log(
                            $"Updating enemy '{enemy.name}' loot item '{lootItem.name}' spriteId to backend spriteId='{backendId}' (loot table '{enemyLootTable.name}')",
                            context,
                            Logging.LogType.Info
                        );
                        lootItem.spriteId = backendId;
                    }
                }
            }
        }
    }

    private static Task<(SpriteCreatedData data, string error)> UploadEnemySpriteAsync(
        EnemiesService enemiesService,
        MonoBehaviour coroutineHost,
        byte[] fileBytes,
        string filename,
        string mimeType
    )
    {
        var tcs = new TaskCompletionSource<(SpriteCreatedData data, string error)>();

        coroutineHost.StartCoroutine(
            enemiesService.UploadEnemySprite(
                fileBytes,
                filename,
                mimeType,
                (data, error) =>
                {
                    tcs.SetResult((data, error));
                }
            )
        );

        return tcs.Task;
    }

    /// <summary>
    /// Upload enemy sprites for all enemies in the world.
    /// </summary>
    public static async Task PublishEnemies(
        WorldData worldData,
        EnemiesService enemiesService,
        Logging.Logger logger,
        MonoBehaviour context
    )
    {
        if (worldData == null || worldData.enemies == null || enemiesService == null)
            return;

        foreach (var enemy in worldData.enemies)
        {
            if (enemy == null || string.IsNullOrEmpty(enemy.spriteId))
                continue;

            logger.Log($"Uploading sprite for enemy '{enemy.name}'", context, Logging.LogType.Info);

            string path = Path.Combine(
                Application.streamingAssetsPath,
                "Items",
                enemy.spriteId + ".png"
            );

            if (!File.Exists(path))
            {
                logger.Log(
                    $"Sprite file not found for enemy '{enemy.name}' at path '{path}'. Skipping upload.",
                    context,
                    Logging.LogType.Warning
                );
                continue;
            }

            byte[] spriteBytes = File.ReadAllBytes(path);
            if (spriteBytes == null || spriteBytes.Length == 0)
            {
                logger.Log(
                    $"Sprite bytes for enemy spriteId '{enemy.spriteId}' are null or empty. Skipping upload.",
                    context,
                    Logging.LogType.Warning
                );
                continue;
            }

            string type = Path.GetExtension(path).Replace(".", "").ToLower();
            var (createdSprite, enemyError) = await UploadEnemySpriteAsync(
                enemiesService,
                context,
                spriteBytes,
                $"{enemy.name}{Path.GetExtension(path)}",
                $"image/{type}"
            );

            if (!string.IsNullOrEmpty(enemyError) || createdSprite == null)
            {
                logger.Log(
                    $"Failed to upload sprite for enemy '{enemy.name}': {enemyError}",
                    context,
                    Logging.LogType.Warning
                );
                continue;
            }

            logger.Log(
                $"Sprite uploaded successfully for enemy '{enemy.name}'",
                context,
                Logging.LogType.Info
            );
        }
    }
}
