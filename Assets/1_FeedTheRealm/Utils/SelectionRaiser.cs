using System;
using Models;

namespace Utils
{
    public static class SelectionRaiser
    {
        public static event Action<IPlaceable> ObjectSelected;

        public static void RaiseSelected(IPlaceable reference)
        {
            ObjectSelected?.Invoke(reference);
        }
    }
}
