using FTRShared.Runtime.Models;

namespace FeedTheRealm.Core.DataPersistence
{
    public interface IPersistent
    {
        void SaveData(ref WorldDataOld worldData);
    }

    public interface IPersistent<T>
    {
        void SaveData(ref T data);
    }
}
