using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API;
using Models;
using UnityEngine;

public class WorldPublisherController : MonoBehaviour
{
    [SerializeField]
    Logging.Logger logger;

    [SerializeField]
    StructureLoaderSO structureLoader;

    [SerializeField]
    CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    WorldService worldService;

    [SerializeField]
    SpriteService spriteService;

    [SerializeField]
    ModelService modelService;

    [SerializeField]
    Session.Session session;

    /// <summary>
    /// Publishes the world, its models and sprites to the server.
    /// If any step fails, returns the error message.
    /// </summary>
    public async Task<(string, string, long)> PublishWorld(
        WorldData worldData,
        string worldFile,
        string description
    )
    {
        (string worldId, string worldError, long statusCode) = await worldService.PublishWorld(
            worldData,
            worldFile,
            description,
            session.APIToken
        );
        if (!string.IsNullOrEmpty(worldError))
            return (null, worldError, statusCode);

        string modelError = await PublishModels(worldData, worldId);
        if (!string.IsNullOrEmpty(modelError))
            return (null, modelError, 0);
        // TODO: maybe we can consider seperateing this into multiple calls
        List<CreatorObject> creatorObjects = creatorObjectLibrary.GetAllCreatorObjects();
        var spriteData = new Dictionary<string, string>();
        foreach (var obj in creatorObjects)
        {
            spriteData[obj.ObjectId] = obj.spriteFile;
        }
        if (worldData.consumableItems != null)
        {
            foreach (var item in worldData.consumableItems)
            {
                if (!string.IsNullOrEmpty(item.spriteFilepath) && !spriteData.ContainsKey(item.id))
                {
                    spriteData[item.id] = item.spriteFilepath;
                }
            }
        }
        if (worldData.weaponItems != null)
        {
            foreach (var item in worldData.weaponItems)
            {
                if (!string.IsNullOrEmpty(item.spriteFilepath) && !spriteData.ContainsKey(item.id))
                {
                    spriteData[item.id] = item.spriteFilepath;
                }
            }
        }
        var spriteList = spriteData.Select(kvp => (kvp.Key, kvp.Value)).ToList();
        string spriteError = await PublishSprites(spriteList, worldId);
        if (!string.IsNullOrEmpty(spriteError))
            return (null, spriteError, 0);

        return (worldId, null, 200);
    }

    /// <summary>
    ///  Uploads model files for a world.
    ///  If an error occurs, returns the error message.
    /// </summary>
    private async Task<string> PublishModels(WorldData worldData, string worldId)
    {
        if (worldData.objectPlacementData.Count == 0)
            return null;
        foreach (var structure in worldData.objectPlacementData)
        {
            structure.structureFilepath = structureLoader.GetModelFilePath(structure.structureName);
        }
        return await modelService.UploadModels(
            worldData.objectPlacementData,
            worldId,
            session.APIToken
        );
    }

    /// <summary>
    ///  Uploads sprite files for a world.
    ///  The sprites are composed of tuples with (sprite_id, sprite_filepath)
    ///  If an error occurs, returns the error message.
    /// </summary>
    private async Task<string> PublishSprites(List<(string, string)> spriteData, string worldId)
    {
        if (spriteData.Count == 0)
            return null;
        string result = await spriteService.UploadSprites(spriteData, worldId, session.APIToken);
        // Ignores error 409 (conflict) due to duplicate key
        if (!string.IsNullOrEmpty(result) && result.Contains("409"))
        {
            logger.Log(
                "Some sprites already exist on the server (409 conflict). Continuing with publication.",
                this,
                Logging.LogType.Warning
            );
            return null;
        }
        return result;
    }
}
