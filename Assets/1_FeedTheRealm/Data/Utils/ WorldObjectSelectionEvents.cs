using System;

namespace Utils
{
    public static class WorldObjectSelectionEvents
    {
        public static event Action<WorldObjectDefinition> ObjectSelected;

        public static void RaiseObjectSelected(WorldObjectDefinition reference)
        {
            ObjectSelected?.Invoke(reference);
        }
    }
}
