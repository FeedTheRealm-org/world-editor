
public interface IDataPersistence {
    void SaveData(ref WorldData worldData);
    void LoadData(WorldData worldData);
}