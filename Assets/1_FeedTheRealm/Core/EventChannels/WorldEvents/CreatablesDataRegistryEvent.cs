using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Core.EventChannels.WorldEvents
{
    [CreateAssetMenu(menuName = "Events/Persistence/CreatablesDataRegistry")]
    public class CreatablesDataRegistryEvent : EventChannelSO<IPersistent<CreatablesData>> { }
}
