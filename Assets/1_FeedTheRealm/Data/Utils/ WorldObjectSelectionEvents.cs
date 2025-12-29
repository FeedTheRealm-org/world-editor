using System;

namespace Utils
{
    public static class WorldObjectSelectionEvents
    {
        public static event Action<IPlaceable> ObjectSelected;

        public static void RaiseObjectSelected(IPlaceable reference)
        {
            ObjectSelected?.Invoke(reference);
        }
    }
}
