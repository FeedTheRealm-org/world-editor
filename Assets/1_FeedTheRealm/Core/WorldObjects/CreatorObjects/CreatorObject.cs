using System;
using System.IO;
using Models;
using UnityEngine;
using Utils;

public abstract class CreatorObject : ICreatable, IPersistent
{
    public string name;
    public string objectId;
    public string spriteFile = "";
    private bool _isDeleted = false;

    private readonly string saveDirectory = "Sprites";

    public CreatorObject(string name, string objectId, string spriteFile = null)
    {
        this.name = name;
        this.objectId = string.IsNullOrEmpty(objectId) ? Guid.NewGuid().ToString() : objectId;
        this.spriteFile = spriteFile;
    }

    public bool IsDeleted => _isDeleted;
    public string ObjectId => objectId;
    public string DisplayName => name;

    public void Delete()
    {
        _isDeleted = true;
    }

    public abstract void SaveObject(ref WorldData worldData);

    public abstract void DeleteObject(ref WorldData worldData);

    public void SaveData(ref WorldData worldData)
    {
        Debug.Log($"Saving CreatorObject: {DisplayName} (ID: {ObjectId}), IsDeleted: {_isDeleted}");
        if (_isDeleted)
        {
            DeleteSprite();
            DeleteObject(ref worldData);
        }
        else
        {
            SaveSprite();
            SaveObject(ref worldData);
        }
    }

    private void SaveSprite()
    {
        if (string.IsNullOrEmpty(spriteFile))
            return;
        string sourceSpritePath = spriteFile;
        if (!Path.IsPathRooted(spriteFile))
        {
            sourceSpritePath = Path.Combine(Application.streamingAssetsPath, spriteFile);
        }
        Debug.Log(
            $"Saving sprite for CreatorObject: {DisplayName} (ID: {ObjectId}) from path: {sourceSpritePath}"
        );
        spriteFile = FileHandler.SaveFile(sourceSpritePath, saveDirectory, objectId);
        Debug.Log($"Sprite saved at path: {spriteFile}");
    }

    private void DeleteSprite()
    {
        if (string.IsNullOrEmpty(spriteFile))
            return;
        FileHandler.DeleteFile(spriteFile);
        spriteFile = null;
    }

    public override string ToString()
    {
        return $"CreatorObject(Name: {name}, ID: {objectId}, SpriteFile: {spriteFile}, IsDeleted: {_isDeleted})";
    }
}
