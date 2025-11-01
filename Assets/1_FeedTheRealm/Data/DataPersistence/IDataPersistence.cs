using UnityEngine;

public interface IDataPersistence
{
    void LoadData(WorldData data);
    void SaveData(ref WorldData data);
}
