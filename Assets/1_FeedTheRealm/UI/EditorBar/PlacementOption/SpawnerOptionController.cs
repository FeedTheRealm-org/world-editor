using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects.PlaceableObjects;
using FeedTheRealm.UI.Common;
using UnityEngine;

namespace FeedTheRealm.UI.EditorBar.PlacementOption
{
    public class SpawnerOptionController : MenuOption, ICategoryOption
    {
        private CategorySelectedEvent categorySelectedEvent;

        public void SetCategoryEvent(CategorySelectedEvent evt) => categorySelectedEvent = evt;

        public override void Execute()
        {
            categorySelectedEvent?.Raise(PlaceableObjectCategories.Spawner);
        }
    }
}
