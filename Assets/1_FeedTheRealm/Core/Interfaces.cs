using System;
using System.Threading.Tasks;
using FeedTheRealm.Core.Interfaces;
using UnityEngine;

namespace FeedTheRealm.Core.Interfaces
{
    public interface IEditable
    {
        void OnObjectSelected(Action CloseEditorCallback);

        void OnObjectDeselected();
    }

    public interface IPlaceable
    {
        string DisplayName { get; }
        Task<GameObject> GetPlaceableObject(int layerMask);
    }

    public interface ISetup
    {
        void Setup();
    }

    public interface IWorldEditorState
    {
        void Enter();
        void Exit();
        void Tick(); // used for continuous updates (hovering, preview ghosts, raycasts)
        void OnPrimaryAction(); // left click
        void OnSecondaryAction(); // right click
    }
}
