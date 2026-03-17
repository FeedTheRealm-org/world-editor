using System.Collections.Generic;
using System.Runtime.InteropServices;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Interfaces;
using FeedTheRealm.Core.WorldObjects.PlaceableObjects;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.EditorBar
{
    public class PlaceableObjectDisplayController : MonoBehaviour
    {
        [SerializeField]
        private PlaceablesLibrary placeableObjectLibrary;

        [Inject]
        private CategorySelectedEvent categorySelectedEvent;

        [Inject]
        private ObjectSelectedEvent objectSelectedEvent;

        [Inject]
        private EnableInputEvent enableInputEvent;
        private ListView libraryBar;
        private List<IPlaceable> currentObjectList = new();
        private PlaceableObjectCategories currentCategory = PlaceableObjectCategories.Structure;

        void Start()
        {
            UIDocument hudVisualDocument = GetComponent<UIDocument>();
            if (hudVisualDocument == null)
            {
                Debug.LogError("UIDocument component not found on " + gameObject.name);
                return;
            }

            libraryBar = hudVisualDocument.rootVisualElement.Q<ListView>("LibraryBar");
            if (libraryBar == null)
            {
                Debug.LogError("ListView 'LibraryBar' not found in UXML");
                return;
            }

            categorySelectedEvent.OnRaised += LoadObjectsForCategory;

            InitializeLibraryBar();
            LoadObjectsForCategory(PlaceableObjectCategories.Structure);
        }

        void OnDestroy()
        {
            categorySelectedEvent.OnRaised -= LoadObjectsForCategory;
        }

        private void InitializeLibraryBar()
        {
            currentObjectList = placeableObjectLibrary.GetObjects(currentCategory);

            libraryBar.itemsSource = currentObjectList;
            libraryBar.makeItem = MakeListItem;
            libraryBar.bindItem = BindListItem;
        }

        private VisualElement MakeListItem()
        {
            var button = new Button();
            button.AddToClassList("placeableObjectOption");
            button.style.width = Length.Percent(100);

            button.RegisterCallback<MouseEnterEvent>(evt =>
            {
                enableInputEvent.Raise(false);
            });
            button.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                enableInputEvent.Raise(true);
            });

            return button;
        }

        private void BindListItem(VisualElement element, int index)
        {
            if (element is Button button && index < currentObjectList.Count)
            {
                var placeable = currentObjectList[index];
                button.text = placeable.DisplayName;
                button.clicked += () =>
                {
                    objectSelectedEvent.Raise(placeable);
                };
            }
        }

        private void LoadObjectsForCategory(PlaceableObjectCategories category)
        {
            currentCategory = category;
            currentObjectList = placeableObjectLibrary.GetObjects(category);
            libraryBar.itemsSource = currentObjectList;
            libraryBar.Rebuild();
        }
    }
}
