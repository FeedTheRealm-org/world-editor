using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels;
using UnityEngine;

namespace FeedTheRealm.Core.EventChannels.WorldEvents
{
    [CreateAssetMenu(menuName = "Events/DataPersistenceRegistry")]
    public class DataPersistenceRegistryEvent : EventChannelSO<IPersistent> { }
}
