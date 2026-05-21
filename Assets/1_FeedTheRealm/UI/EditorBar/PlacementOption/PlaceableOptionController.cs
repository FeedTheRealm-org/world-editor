using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.Library;
using FTR.UI;
using UnityEngine;

namespace FeedTheRealm.UI.EditorBar.PlacementOption
{
    public class PlaceableOptionController : MenuOption, ICategoryOption
    {
        [SerializeField]
        private PlaceableObjectCategories category;

        private CategorySelectedEvent categorySelectedEvent;

        public void SetCategoryEvent(CategorySelectedEvent evt) => categorySelectedEvent = evt;

        public override void Execute()
        {
            categorySelectedEvent?.Raise(category);
        }
    }
}
