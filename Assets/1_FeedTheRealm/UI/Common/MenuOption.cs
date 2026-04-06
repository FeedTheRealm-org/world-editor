using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.Common
{
    public class MenuOption : MonoBehaviour
    {
        [Header("Menu Option Customization")]
        [SerializeField]
        private string label = "Menu Option";

        [Header("Open Menu Tab")]
        [Tooltip(
            "Optional reference to a menu controller that will be opened when this menu option is clicked.\nLeave unassigned if no menu should be opened on interaction."
        )]
        [SerializeField]
        private MenuController menuToOpen;

        [Header("Dropdown Menu Options")]
        [Tooltip(
            "List of submenu options to display in a dropdown when this menu option is clicked. Can be left empty if no dropdown menu is desired."
        )]
        [SerializeField]
        private List<MenuOption> menuOptions = new();

        [Header("Execute Dependencies")]
        public string Label => label;
        public IReadOnlyList<MenuOption> MenuOptions => menuOptions;
        public MenuController MenuToOpen => menuToOpen;

        [Inject]
        private IObjectResolver objectResolver;

        public virtual async void Execute()
        {
            if (menuToOpen != null)
            {
                Transform parent = FindFirstObjectByType<Canvas>()?.transform;
                if (parent != null)
                    objectResolver.Instantiate(menuToOpen, parent);
                else
                    objectResolver.Instantiate(menuToOpen);
            }
        }
    }
}
