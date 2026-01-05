using System;
using Models;
using UnityEngine;

public abstract class CreatorObject : ICreatable, IPersistent
{
    public string name;
    public string objectId;
    private bool _isDeleted = false;

    public CreatorObject(string name, string objectId)
    {
        this.name = name;
        this.objectId = string.IsNullOrEmpty(objectId) ? Guid.NewGuid().ToString() : objectId;
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
            DeleteObject(ref worldData);
        else
            SaveObject(ref worldData);
    }
}
