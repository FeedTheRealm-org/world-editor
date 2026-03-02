using FeedTheRealm.Core.EventChannels;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/ObjectSelectedEvent")]
public class ObjectSelectedEvent : EventChannelSO<IPlaceable> { }
