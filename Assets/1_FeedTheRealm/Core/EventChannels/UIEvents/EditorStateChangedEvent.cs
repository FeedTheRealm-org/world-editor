using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldEditor;
using UnityEngine;

namespace FeedTheRealm.Core.EventChannels.UIEvents
{
    [CreateAssetMenu(menuName = "Events/UI/Editor State Changed")]
    public class EditorStateChangedEvent : EventChannelSO<EditorStates> { }
}
