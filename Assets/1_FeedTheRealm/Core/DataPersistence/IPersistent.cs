using FTRShared.Runtime.Models;

namespace FeedTheRealm.Core.DataPersistence
{
    public interface IPersistent
    {
        void SaveData(ref WorldData worldData);
    }
}
