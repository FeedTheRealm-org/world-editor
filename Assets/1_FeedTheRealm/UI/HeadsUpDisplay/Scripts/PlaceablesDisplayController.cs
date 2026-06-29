using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Gameplay.Inputs;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.HeadsUpDisplay
{
    public class PlaceableObjectDisplayController : MonoBehaviour
    {
        [SerializeField]
        private VisualTreeAsset placeableItemTemplate;

        [SerializeField]
        private List<PlaceableObjectCategories> categoriesToDisplay;

        [Inject]
        private PlaceablesLibrary placeableObjectLibrary;

        [Inject]
        private ObjectSelectedEvent objectSelectedEvent;

        [Inject]
        private EnableInteractionsEvent enableInputEvent;

        [Inject]
        private RefreshPlaceableLibraryEvent refreshPlaceableLibraryEvent;

        [Inject]
        private InputReader inputReader;

        private ScrollView libraryBar;

        private TextField searchField;
        private Button searchClearBtn;
        private DropdownField categoryDropdown;
        private const string DropdownOverlayClass = "unity-base-dropdown__container-outer";

        private bool pointerOverSidebar;
        private bool dropdownOpen;

        private List<PlaceableOption> allItems = new();
        private List<PlaceableOption> filteredItems = new();
        private PlaceableObjectCategories currentCategory = PlaceableObjectCategories.Structure;

        void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var sidebar = root.Q<VisualElement>("root");

            libraryBar = root.Q<ScrollView>("LibraryBar");
            searchField = root.Q<TextField>("SearchField");
            categoryDropdown = root.Q<DropdownField>("CategoryDropdown");

            if (libraryBar == null)
            {
                Debug.LogError("LibraryBar not found");
                return;
            }

            // Category dropdown
            categoryDropdown.choices = categoriesToDisplay.Select(c => c.ToString()).ToList();
            categoryDropdown.value = currentCategory.ToString();
            categoryDropdown.RegisterValueChangedCallback(evt =>
            {
                if (System.Enum.TryParse<PlaceableObjectCategories>(evt.newValue, out var cat))
                {
                    currentCategory = cat;
                    LoadObjectsForCategory(cat);
                }
                searchField.value = string.Empty;
            });

            // The opened dropdown renders as a full-screen overlay outside the sidebar, so
            // the sidebar's PointerLeave fires while it is open. Track it separately so world
            // interaction stays disabled until the menu actually closes.
            categoryDropdown.RegisterCallback<PointerDownEvent>(_ => OnDropdownOpened());

            // Search field
            searchClearBtn = new Button(() =>
            {
                searchField.value = string.Empty;
                searchClearBtn.style.display = DisplayStyle.None;
            });
            searchClearBtn.text = "✕";
            searchClearBtn.AddToClassList("hud-sidebar__search-clear");
            searchClearBtn.style.display = DisplayStyle.None;

            var searchRow = root.Q<VisualElement>("SearchRow");
            searchRow?.Add(searchClearBtn);

            searchField.RegisterValueChangedCallback(evt =>
            {
                searchClearBtn.style.display = string.IsNullOrEmpty(evt.newValue)
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
                ApplySearch(evt.newValue);
            });

            searchField.RegisterCallback<FocusInEvent>(_ =>
                inputReader.ToggleExternalInputs(false)
            );
            searchField.RegisterCallback<FocusOutEvent>(_ =>
                inputReader.ToggleExternalInputs(true)
            );
            // While the pointer is over the sidebar, block world interaction so clicking a
            // list item / the dropdown doesn't also select the placeable behind the panel.
            sidebar.RegisterCallback<PointerEnterEvent>(_ =>
            {
                inputReader.ToggleExternalInputs(false);
                pointerOverSidebar = true;
                UpdateWorldInteraction();
            });
            sidebar.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                inputReader.ToggleExternalInputs(true);
                pointerOverSidebar = false;
                UpdateWorldInteraction();
            });

            // Events
            refreshPlaceableLibraryEvent.OnRaised += OnRefresh;

            LoadObjectsForCategory(currentCategory);
            libraryBar.focusable = false;
        }

        void OnDestroy()
        {
            refreshPlaceableLibraryEvent.OnRaised -= OnRefresh;
        }

        // Data loading
        private void LoadObjectsForCategory(PlaceableObjectCategories category)
        {
            allItems = placeableObjectLibrary.GetPlaceableOptions(category);
            ApplySearch(searchField?.value ?? string.Empty);
        }

        private void ApplySearch(string query)
        {
            filteredItems = string.IsNullOrWhiteSpace(query)
                ? new List<PlaceableOption>(allItems)
                : allItems
                    .Where(o =>
                        o.displayName.Contains(query, System.StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();

            libraryBar.Clear();
            foreach (var option in filteredItems)
            {
                var entry = placeableItemTemplate.Instantiate();
                var button = entry.Q<Button>("ItemButton");
                var label = entry.Q<Label>("ItemLabel");

                label.text = option.displayName;
                button.focusable = false;
                button.clicked += () => objectSelectedEvent.Raise(option);
                libraryBar.Add(entry);
            }
        }

        private void OnRefresh() => LoadObjectsForCategory(currentCategory);

        private void UpdateWorldInteraction()
        {
            enableInputEvent.Raise(!(pointerOverSidebar || dropdownOpen));
        }

        private void OnDropdownOpened()
        {
            dropdownOpen = true;
            UpdateWorldInteraction();

            // DropdownField has no "closed" callback. The menu overlay is created during this
            // same pointer-down dispatch, so grab it on the next frame and clear the flag when
            // it detaches from the panel (which happens when the dropdown closes, however it
            // closed). One deferred call + one event beats continuous polling.
            categoryDropdown.schedule.Execute(() =>
            {
                var overlay = categoryDropdown.panel?.visualTree.Q(className: DropdownOverlayClass);
                if (overlay == null)
                {
                    // Menu didn't open (or already gone) — restore world interaction now.
                    dropdownOpen = false;
                    UpdateWorldInteraction();
                    return;
                }

                overlay.RegisterCallback<DetachFromPanelEvent>(_ =>
                {
                    dropdownOpen = false;
                    UpdateWorldInteraction();
                });
            });
        }
    }
}
