using System.Collections.Generic;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.MenuBar
{
    public class EditorBarController : MonoBehaviour
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private UIDocument menuBarUI;

        [Header("Menu Options")]
        [SerializeField]
        private MenuOption ZoneOption;

        [SerializeField]
        private MenuOption PlacementOption;

        [SerializeField]
        private MenuOption ElementOption;
        private VisualElement root;
        private readonly List<VisualElement> openMenus = new();

        void Awake()
        {
            root = menuBarUI.rootVisualElement;
            BindButton("Zone", ZoneOption);
            BindButton("Placement", PlacementOption);
            BindButton("Element", ElementOption);
            root.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
        }

        private void BindButton(string buttonName, MenuOption option)
        {
            Button button = root.Q<Button>(buttonName);

            if (button == null)
                logger.Log(
                    $"Button '{buttonName}' not found in the menu bar.",
                    this,
                    Logging.LogType.Error
                );

            if (option == null)
            {
                button.SetEnabled(false);
                button.style.color = Color.grey;
                return;
            }
            button.text = option.Label;
            button.clicked += () =>
            {
                if (option.MenuOptions.Count == 0)
                    return;
                OpenMenu(button, option.MenuOptions, 0);
            };
        }

        private void OpenMenu(VisualElement anchor, IReadOnlyList<MenuOption> options, int depth)
        {
            CloseMenusFromDepth(depth);
            VisualElement menu = new VisualElement();
            menu.AddToClassList("dropdown");
            menu.style.position = Position.Absolute;
            menu.style.flexDirection = FlexDirection.Column;
            menu.RegisterCallback<PointerDownEvent>(e => e.StopPropagation());

            foreach (MenuOption option in options)
            {
                Button button = new() { text = option.Label };

                if (option.MenuOptions.Count > 0)
                {
                    button.RegisterCallback<PointerEnterEvent>(_ =>
                    {
                        OpenMenu(button, option.MenuOptions, depth + 1);
                    });
                }
                else
                {
                    button.clicked += () =>
                    {
                        CloseAllMenus();
                        option.Execute();
                    };
                }
                menu.Add(button);
            }
            root.Add(menu);
            PositionMenu(menu, anchor, depth);
            openMenus.Add(menu);
        }

        private void PositionMenu(VisualElement menu, VisualElement anchor, int depth)
        {
            Rect bounds = anchor.worldBound;
            if (depth == 0)
            {
                menu.style.left = bounds.x;
                menu.style.top = bounds.yMax + 6;
            }
            else
            {
                menu.style.left = bounds.xMax + 4;
                menu.style.top = bounds.y;
            }
        }

        private void CloseMenusFromDepth(int depth)
        {
            for (int i = openMenus.Count - 1; i >= depth; i--)
            {
                openMenus[i].RemoveFromHierarchy();
                openMenus.RemoveAt(i);
            }
        }

        private void CloseAllMenus()
        {
            CloseMenusFromDepth(0);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (openMenus.Count == 0)
                return;

            Vector2 mousePos = evt.position;
            foreach (var menu in openMenus)
            {
                if (menu.worldBound.Contains(mousePos))
                    return;
            }
            if (IsPointerOverMenuBar(mousePos))
                return;
            const float padding = 15f;
            foreach (var menu in openMenus)
            {
                Rect expandedBounds = menu.worldBound;
                expandedBounds.xMin -= padding;
                expandedBounds.xMax += padding;
                expandedBounds.yMin -= padding;
                expandedBounds.yMax += padding;

                if (expandedBounds.Contains(mousePos))
                    return;
            }
            CloseAllMenus();
        }

        private bool IsPointerOverMenuBar(Vector2 position)
        {
            var menuBar = root.Q<VisualElement>("MenuBarContainer");
            return menuBar != null && menuBar.worldBound.Contains(position);
        }
    }
}
