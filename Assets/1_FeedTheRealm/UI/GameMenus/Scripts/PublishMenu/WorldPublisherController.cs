using System.Collections.Generic;
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
    public async Task<(string, string)> PublishWorld(
        WorldData worldData,
        string worldFile,
        string description
    )
    {
        (string worldId, string worldError) = await worldService.PublishWorld(
            worldData,
            worldFile,
            description,
            session.APIToken
        );
        if (!string.IsNullOrEmpty(worldError))
            return (null, worldError);

        string modelError = await PublishModels(worldData, worldId);
        if (!string.IsNullOrEmpty(modelError))
            return (null, modelError);
        // TODO: maybe we can consider seperateing this into multiple calls
        List<CreatorObject> creatorObjects = creatorObjectLibrary.GetAllCreatorObjects();
        string spriteError = await PublishSprites(creatorObjects, worldId);
        if (!string.IsNullOrEmpty(spriteError))
            return (null, spriteError);

        return (worldId, null);
    }

    /// <summary>
    ///  Uploads model files for a world.
    ///  If an error occurs, returns the error message.
    /// </summary>
    private async Task<string> PublishModels(WorldData worldData, string worldId)
    {
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
    private async Task<string> PublishSprites(List<CreatorObject> creatorObjects, string worldId)
    {
        var spriteData = creatorObjects.ConvertAll(obj => (obj.ObjectId, obj.spriteFile));
        return await spriteService.UploadSprites(spriteData, worldId, session.APIToken);
    }
}
