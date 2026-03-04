using System.Collections.Generic;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

/// <summary>
/// Manages a stack of dropdown menus for the UI. Allows opening submenus on hover and ensures only one menu is open at a time.
/// </summary>
public class MenuStack
{
    private readonly VisualElement root;
    private readonly IObjectResolver resolver;
    private readonly EnableInputEvent enableInputEvent;
    private readonly List<VisualElement> openMenus = new();
    private int menuSpacingX = 20;
    private int menuSpacingY = 6;

    private bool enabled = true;

    public MenuStack(
        VisualElement root,
        EnableEditorEvent enableEditorEvent,
        EnableInputEvent enableInputEvent,
        IObjectResolver resolver
    )
    {
        this.root = root;
        this.resolver = resolver;
        this.enableInputEvent = enableInputEvent;
        enableEditorEvent.OnRaised += ToggleMenuStack;
    }

    public void Open(VisualElement anchor, IReadOnlyList<MenuOption> options, int depth = 0)
    {
        if (!enabled)
            return;
        CloseFromDepth(depth);
        enableInputEvent?.Raise(false);
        var menu = CreateMenu(anchor, options, depth);
        root.Add(menu);
        openMenus.Add(menu);
    }

    // -------------------- Private Methods --------------------
    private void ToggleMenuStack(bool enabled)
    {
        Debug.Log($"Setting menu interaction lock to {(enabled ? "unlocked" : "locked")}.");
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

        // Track which anchor spawned this menu so we can toggle later
        menu.userData = anchor;

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
                    if (option.MenuToOpen != null && resolver != null)
                        resolver.Instantiate(option.MenuToOpen.gameObject);
                    else
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

    /// <summary>
    /// Returns true if the menu at the given depth (default 0) was opened by the specified anchor.
    /// </summary>
    public bool IsOpenForAnchor(VisualElement anchor, int depth = 0)
    {
        if (openMenus.Count > depth)
        {
            var menu = openMenus[depth];
            return menu.userData == anchor;
        }
        return false;
    }

    /// <summary>
    /// Toggle the dropdown for an anchor. If already open, closes it; otherwise opens a new one.
    /// </summary>
    public void Toggle(VisualElement anchor, IReadOnlyList<MenuOption> options)
    {
        if (IsOpenForAnchor(anchor))
            CloseAll();
        else
            Open(anchor, options);
    }

    public void CloseFromDepth(int depth)
    {
        for (int i = openMenus.Count - 1; i >= depth; i--)
        {
            openMenus[i].RemoveFromHierarchy();
            openMenus.RemoveAt(i);
        }
        if (openMenus.Count == 0)
            enableInputEvent?.Raise(true);
    }

    public void CloseAll()
    {
        CloseFromDepth(0);
    }

    public bool AnyOpen => openMenus.Count > 0;
}
