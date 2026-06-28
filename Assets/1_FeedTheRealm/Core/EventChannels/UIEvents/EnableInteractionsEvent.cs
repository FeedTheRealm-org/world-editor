using FeedTheRealm.Core.EventChannels;
using UnityEngine;

namespace FeedTheRealm.Core.EventChannels.UIEvents
{
    [CreateAssetMenu(menuName = "Events/UI/EnableInteractionsEvent")]
    public class EnableInteractionsEvent : EventChannelSO<bool> { }
}
