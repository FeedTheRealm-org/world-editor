using FeedTheRealm.Core.EventChannels;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/CategorySelectedEvent")]
public class CategorySelectedEvent : EventChannelSO<PlaceableObjectCategories> { }
