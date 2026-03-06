using FeedTheRealm.Core.EventChannels;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Core.EventChannels.WorldEvents
{
    [CreateAssetMenu(menuName = "Events/WorldSelectedEvent")]
    public class WorldSelectedEvent : EventChannelSO<WorldData> { }
}
