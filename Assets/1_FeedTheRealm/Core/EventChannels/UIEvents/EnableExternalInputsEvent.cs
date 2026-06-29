using FeedTheRealm.Core.EventChannels;
using UnityEngine;

namespace FeedTheRealm.Core.EventChannels.UIEvents
{
    [CreateAssetMenu(menuName = "Events/UI/EnableExternalInputsEvent")]
    public class EnableExternalInputsEvent : EventChannelSO<bool> { }
}
