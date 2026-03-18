using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FTRShared.Runtime.Models;

public abstract class WorldObject : IPersistent
{
    public WorldObject(DataPersistenceRegistryEvent registryEvent)
    {
        registryEvent.Raise(this);
    }

    public abstract void SaveData(ref WorldData worldData);
}
