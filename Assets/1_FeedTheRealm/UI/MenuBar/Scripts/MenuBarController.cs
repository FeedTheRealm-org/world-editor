using System.Collections.Generic;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.MenuBar
{
    public class MenuBarController : MonoBehaviour
    {
        [SerializeField]
        private UIDocument menuBarUI;

        [SerializeField]
        private List<MenuOption> rootOptions;

        [SerializeField]
        private MenuOption fileOptionController;

        [SerializeField]
        private MenuOption editOptionController;

        [SerializeField]
        private MenuOption subscriptionsOptionController;

        [SerializeField]
        private MenuOption helpOptionController;

        [SerializeField]
        private MenuOption aboutOptionController;

        private VisualElement root;
        private VisualElement currentDropdown;
        private VisualElement clickCatcher;

        void Awake()
        {
            root = menuBarUI.rootVisualElement;
            BindButton("File", fileOptionController);
            BindButton("Edit", editOptionController);
            BindButton("Subscriptions", subscriptionsOptionController);
            BindButton("Help", helpOptionController);
            BindButton("About", aboutOptionController);

            root.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        }

        private void BindButton(string buttonName, MenuOption option)
        {
            var button = root.Q<Button>(buttonName);
            if (button == null)
            {
                Debug.LogWarning($"Menu button '{buttonName}' not found in UXML.");
                return;
            }
            button.clicked += () =>
            {
                CloseDropdown();

                if (option == null)
                    return;
                if (option.MenuOptions.Count == 0)
                {
                    option.Execute();
                    return;
                }
                OpenDropdown(button, option);
            };
        }

        private void OpenDropdown(Button source, MenuOption option)
        {
            var dropdown = new VisualElement();
            dropdown.AddToClassList("dropdown");
            dropdown.style.position = Position.Absolute;

            foreach (var sub in option.MenuOptions)
            {
                var btn = new Button(() =>
                {
                    CloseDropdown();
                    sub.Execute();
                })
                {
                    text = sub.Label,
                };
                btn.style.width = Length.Percent(100);
                dropdown.Add(btn);
            }

            root.Add(dropdown);

            // Position under button
            var bounds = source.worldBound;
            dropdown.style.width = bounds.width;
            dropdown.style.left = bounds.x;
            dropdown.style.top = bounds.yMax + 4;

            currentDropdown = dropdown;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (currentDropdown == null)
                return;

            if (currentDropdown.worldBound.Contains(evt.position))
                return;

            CloseDropdown();
        }

        private void CloseDropdown()
        {
            currentDropdown?.RemoveFromHierarchy();
            currentDropdown = null;
        }
    }
}
