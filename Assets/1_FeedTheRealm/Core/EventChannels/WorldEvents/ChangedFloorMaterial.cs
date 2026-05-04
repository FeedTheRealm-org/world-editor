using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.Library;
using UnityEngine;

namespace FeedTheRealm.Core.EventChannels.UIEvents
{
    [CreateAssetMenu(menuName = "Events/Changed Floor Material")]
    public class ChangedFloorMaterialEvent : EventChannelSO<Material> { }
}
