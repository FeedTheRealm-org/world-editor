using FeedTheRealm.Core.EventChannels;
using Models;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/WorldSelectedEvent")]
public class WorldSelectedEvent : EventChannelSO<WorldData> { }
