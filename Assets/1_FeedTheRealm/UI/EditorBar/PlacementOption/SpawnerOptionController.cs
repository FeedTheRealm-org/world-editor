using FeedTheRealm.UI.Common;
using UnityEngine;

public class SpawnerOptionController : MenuOption, ICategoryOption
{
    private CategorySelectedEvent categorySelectedEvent;

    public void SetCategoryEvent(CategorySelectedEvent evt) => categorySelectedEvent = evt;

    public override void Execute()
    {
        categorySelectedEvent?.Raise(PlaceableObjectCategories.Spawner);
    }
}
