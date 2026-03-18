using System.Collections.Generic;

namespace FeedTheRealm.Core.Library
{
    public enum PlaceableObjectCategories
    {
        Structure,
        Spawner,
    }

    public static class PlaceableObjectCategoriesExtensions
    {
        public static List<PlaceableObjectCategories> GetPlaceableObjectCategories =>
            new() { PlaceableObjectCategories.Structure, PlaceableObjectCategories.Spawner };
    }
}
