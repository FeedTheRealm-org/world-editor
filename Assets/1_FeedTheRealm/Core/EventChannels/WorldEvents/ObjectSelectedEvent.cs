using FeedTheRealm.Core.WorldEditor;
using UnityEngine;

namespace FeedTheRealm.Core.EventChannels.WorldEvents
{
    [CreateAssetMenu(menuName = "Events/ObjectSelectedEvent")]
    public class ObjectSelectedEvent : EventChannelSO<PlaceableOption> { }
}
