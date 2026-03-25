using FeedTheRealm.Core.EventChannels.Ticks;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Core.EventChannels
{
    /// <summary>
    /// A single ScriptableObject that holds references to all event channels in the project.
    /// Assign all event SO assets here once, then pass this registry to the DI container
    /// via RegisterAll() instead of registering each event individually.
    /// </summary>
    [CreateAssetMenu(fileName = "EventChannelRegistry", menuName = "Events/EventChannelRegistry")]
    public class EventChannelRegistry : ScriptableObject
    {
        public LoadWorldEvent loadWorldEvent;

        [Header("World Events")]
        public WorldSelectedEvent worldSelectedEvent;
        public ObjectSelectedEvent objectSelectedEvent;
        public EnableEditorEvent enableEditorEvent;
        public ZoneDataRegistryEvent ZoneDataRegistryEvent;
        public CreatablesDataRegistryEvent CreatablesDataRegistryEvent;
        public EditPlaceableEvent editPlaceableEvent;

        [Header("UI Events")]
        public CategorySelectedEvent categorySelectedEvent;
        public EnableInputEvent enableInputEvent;

        [Header("Tick Events")]
        public TickEvent tickEvent;
        public FixedTickEvent fixedTickEvent;
        public LateTickEvent lateTickEvent;

        /// <summary>
        /// Registers all event channels as singleton instances in the VContainer builder.
        /// Call this once from LifetimeScope.Configure() instead of registering each event manually.
        /// </summary>
        public void RegisterAll(IContainerBuilder builder)
        {
            builder.RegisterInstance(editPlaceableEvent);
            builder.RegisterInstance(ZoneDataRegistryEvent);
            builder.RegisterInstance(CreatablesDataRegistryEvent);
            builder.RegisterInstance(worldSelectedEvent);
            builder.RegisterInstance(objectSelectedEvent);
            builder.RegisterInstance(enableEditorEvent);
            builder.RegisterInstance(categorySelectedEvent);
            builder.RegisterInstance(enableInputEvent);
            builder.RegisterInstance(tickEvent);
            builder.RegisterInstance(fixedTickEvent);
            builder.RegisterInstance(lateTickEvent);
            builder.RegisterInstance(loadWorldEvent);
        }
    }
}
