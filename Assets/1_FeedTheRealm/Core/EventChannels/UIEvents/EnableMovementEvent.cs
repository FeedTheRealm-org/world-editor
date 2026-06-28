using FeedTheRealm.Core.EventChannels;
using UnityEngine;

namespace FeedTheRealm.Core.EventChannels.UIEvents
{
    [CreateAssetMenu(menuName = "Events/UI/EnableMovementEvent")]
    public class EnableMovementEvent : EventChannelSO<bool> { }
}
