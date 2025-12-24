using System;

public static class WorldObjectSelectionEvents
{
    public static event Action<WorldObjectReference> ObjectSelected;

    public static void RaiseObjectSelected(WorldObjectReference reference)
    {
        ObjectSelected?.Invoke(reference);
    }
}
