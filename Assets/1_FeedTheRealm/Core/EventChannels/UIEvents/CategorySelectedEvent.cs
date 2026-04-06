using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.Library;
using UnityEngine;

namespace FeedTheRealm.Core.EventChannels.UIEvents
{
    [CreateAssetMenu(menuName = "Events/CategorySelectedEvent")]
    public class CategorySelectedEvent : EventChannelSO<PlaceableObjectCategories> { }
}
