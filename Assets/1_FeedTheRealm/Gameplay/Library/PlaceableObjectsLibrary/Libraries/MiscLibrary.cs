using System.Collections.Generic;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Core.WorldObjects.Provider;
using VContainer;

namespace FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary
{
    public class MiscLibrary : PrefabLibrary
    {
        public MiscLibrary(WorldPrefabProvider prefabProvider, IObjectResolver resolver)
            : base(resolver)
        {
            Register(PlaceableOptions.Portal, prefabProvider.portalPrefab);
        }

        public override List<PlaceableOption> ListAvailableItems() =>
            new()
            {
                new()
                {
                    category = PlaceableObjectCategories.Misc,
                    id = PlaceableOptions.Portal,
                    displayName = PlaceableOptions.Portal,
                },
            };
    }
}
