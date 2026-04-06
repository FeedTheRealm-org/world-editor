using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.WorldEditor;
using UnityEngine;

namespace FeedTheRealm.Core.EventChannels.WorldEvents
{
    [CreateAssetMenu(menuName = "Events/EditPlaceableEvent")]
    public class EditPlaceableEvent : EventChannelSO<EditableOption> { }
}
