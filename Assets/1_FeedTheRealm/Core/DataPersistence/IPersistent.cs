namespace FeedTheRealm.Core.DataPersistence
{
    public interface IPersistent<T>
    {
        void SaveData(ref T data);
    }
}
