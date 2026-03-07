using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.Interfaces;
using UnityEngine;

namespace FeedTheRealm.Core.EventChannels.WorldEvents
{
    [CreateAssetMenu(menuName = "Events/ObjectSelectedEvent")]
    public class ObjectSelectedEvent : EventChannelSO<IPlaceable> { }
}
