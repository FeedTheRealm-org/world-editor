using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.WorldObjects.PlaceableObjects;
using UnityEngine;

namespace FeedTheRealm.Core.EventChannels.UIEvents
{
    [CreateAssetMenu(menuName = "Events/CategorySelectedEvent")]
    public class CategorySelectedEvent : EventChannelSO<PlaceableObjectCategories> { }
}
