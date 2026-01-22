using System;
using Models;
using UnityEngine;

namespace Utils
{
    public static class SelectionRaiser
    {
        public static event Action<IPlaceable> ObjectSelected;

        public static event Action<WorldData> WorldSelected;

        public static event Action<GameObject> GameObjectSelected;

        public static void RaiseSelected(IPlaceable reference)
        {
            ObjectSelected?.Invoke(reference);
        }

        public static void RaiseSelected(WorldData reference)
        {
            WorldSelected?.Invoke(reference);
        }

        public static void RaiseSelected(GameObject reference)
        {
            GameObjectSelected?.Invoke(reference);
        }
    }
}
