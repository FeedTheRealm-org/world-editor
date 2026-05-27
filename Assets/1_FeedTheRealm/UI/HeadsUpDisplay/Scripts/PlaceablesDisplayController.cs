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
        private EnableInputEvent enableInputEvent;

        [Inject]
        private RefreshPlaceableLibraryEvent refreshPlaceableLibraryEvent;

        [Inject]
        private InputReader inputReader;

        private ScrollView libraryBar;

        private TextField searchField;
        private Button searchClearBtn;
        private DropdownField categoryDropdown;

        private List<PlaceableOption> allItems = new();
        private List<PlaceableOption> filteredItems = new();
        private PlaceableObjectCategories currentCategory = PlaceableObjectCategories.Structure;

        void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

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
            });

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

            searchField.RegisterCallback<FocusInEvent>(_ => inputReader.ToggleInput(false));
            searchField.RegisterCallback<FocusOutEvent>(_ => inputReader.ToggleInput(true));

            // Events
            refreshPlaceableLibraryEvent.OnRaised += OnRefresh;

            LoadObjectsForCategory(currentCategory);
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
                button.clicked += () => objectSelectedEvent.Raise(option);
                button.RegisterCallback<MouseEnterEvent>(_ => enableInputEvent.Raise(false));
                button.RegisterCallback<MouseLeaveEvent>(_ => enableInputEvent.Raise(true));

                libraryBar.Add(entry);
            }
        }

        private void OnRefresh() => LoadObjectsForCategory(currentCategory);
    }
}
