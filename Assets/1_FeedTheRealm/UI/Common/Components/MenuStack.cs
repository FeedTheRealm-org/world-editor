using System.Collections.Generic;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Manages a stack of dropdown menus for the UI. Allows opening submenus on hover and ensures only one menu is open at a time.
/// </summary>
public class MenuStack
{
    private readonly VisualElement root;
    private readonly List<VisualElement> openMenus = new();
    private int menuSpacingX = 20;
    private int menuSpacingY = 6;

    private bool enabled = true;

    public MenuStack(VisualElement root)
    {
        this.root = root;
        Utils.SelectionRaiser.EnableEditor += ToggleMenuStack;
    }

    public void Open(VisualElement anchor, IReadOnlyList<MenuOption> options, int depth = 0)
    {
        if (!enabled)
            return;
        CloseFromDepth(depth);
        var menu = CreateMenu(anchor, options, depth);
        root.Add(menu);
        openMenus.Add(menu);
    }

    // -------------------- Private Methods --------------------
    private void ToggleMenuStack(bool enabled)
    {
        Debug.Log($"Setting menu interaction lock to {(enabled ? "locked" : "unlocked")}.");
        this.enabled = enabled;
        if (!enabled)
            CloseAll();
    }

    private VisualElement CreateMenu(
        VisualElement anchor,
        IReadOnlyList<MenuOption> options,
        int depth
    )
    {
        var menu = new VisualElement();
        menu.AddToClassList("dropdown");
        menu.style.position = Position.Absolute;
        menu.style.flexDirection = FlexDirection.Column;

        menu.RegisterCallback<PointerDownEvent>(e => e.StopPropagation());
        menu.RegisterCallback<PointerLeaveEvent>(_ => CloseFromDepth(depth));

        foreach (var option in options)
        {
            var button = new Button { text = option.Label };
            button.AddToClassList("dropdown-item");

            if (option.MenuOptions.Count > 0)
            {
                button.RegisterCallback<PointerEnterEvent>(_ =>
                {
                    Open(button, option.MenuOptions, depth + 1);
                });
            }
            else
            {
                button.clicked += () =>
                {
                    CloseAll();
                    option.Execute();
                };
            }

            menu.Add(button);
        }

        PositionMenu(menu, anchor, depth);
        return menu;
    }

    private void PositionMenu(VisualElement menu, VisualElement anchor, int depth)
    {
        Rect bounds = anchor.worldBound;

        if (depth == 0)
        {
            menu.style.left = bounds.x + menuSpacingX;
            menu.style.top = bounds.yMax + menuSpacingY;
        }
        else
        {
            menu.style.left = bounds.xMax + menuSpacingX;
            menu.style.top = bounds.y;
        }
    }

    public void CloseFromDepth(int depth)
    {
        for (int i = openMenus.Count - 1; i >= depth; i--)
        {
            openMenus[i].RemoveFromHierarchy();
            openMenus.RemoveAt(i);
        }
    }

    public void CloseAll()
    {
        CloseFromDepth(0);
    }

    public bool AnyOpen => openMenus.Count > 0;
}
