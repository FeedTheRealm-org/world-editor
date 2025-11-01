using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class AssetDatabaseSO : ScriptableObject {

    public List<ObjectData> objectData;


    public ObjectData GetObjectDataById(int id) {
        return objectData.Find(obj => obj.Id == id);
    }
}
