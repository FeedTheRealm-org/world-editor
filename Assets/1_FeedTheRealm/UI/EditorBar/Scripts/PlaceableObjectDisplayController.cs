using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Interfaces;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Core.WorldObjects.PlaceableObjects;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.EditorBar
{
    public class PlaceableObjectDisplayController : MonoBehaviour
    {
        [Inject]
        private PlaceablesLibrary placeableObjectLibrary;

        [Inject]
        private CategorySelectedEvent categorySelectedEvent;

        [Inject]
        private ObjectSelectedEvent objectSelectedEvent;

        [Inject]
        private EnableInputEvent enableInputEvent;

        private ListView libraryBar;
        private List<string> currentItems = new();
        private PlaceableObjectCategories currentCategory = PlaceableObjectCategories.Structure;

        void Start()
        {
            var hudVisualDocument = GetComponent<UIDocument>();
            if (hudVisualDocument == null)
            {
                Debug.LogError("UIDocument not found on " + gameObject.name);
                return;
            }

            libraryBar = hudVisualDocument.rootVisualElement.Q<ListView>("LibraryBar");
            if (libraryBar == null)
            {
                Debug.LogError("ListView 'LibraryBar' not found in UXML");
                return;
            }

            libraryBar.makeItem = MakeListItem;
            libraryBar.bindItem = BindListItem;
            libraryBar.itemsSource = currentItems;

            categorySelectedEvent.OnRaised += LoadObjectsForCategory;
            LoadObjectsForCategory(PlaceableObjectCategories.Structure);
        }

        void OnDestroy()
        {
            categorySelectedEvent.OnRaised -= LoadObjectsForCategory;
        }

        private VisualElement MakeListItem()
        {
            var button = new Button();
            button.AddToClassList("placeableObjectOption");
            button.style.width = Length.Percent(100);
            button.RegisterCallback<MouseEnterEvent>(_ => enableInputEvent.Raise(false));
            button.RegisterCallback<MouseLeaveEvent>(_ => enableInputEvent.Raise(true));
            return button;
        }

        private void BindListItem(VisualElement element, int index)
        {
            if (element is not Button button)
                return;
            string id = currentItems[index];
            button.text = id;
            button.clicked += () =>
                objectSelectedEvent.Raise(
                    new SelectedPlaceable { category = currentCategory, id = id }
                );
        }

        private void LoadObjectsForCategory(PlaceableObjectCategories category)
        {
            currentCategory = category;
            currentItems = placeableObjectLibrary.GetPlaceableOptions(category).Keys.ToList();
            libraryBar.itemsSource = currentItems;
            libraryBar.Rebuild();
        }
    }
}
