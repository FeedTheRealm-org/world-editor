using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlaceableObjectDisplayController : MonoBehaviour
{
    [SerializeField]
    private PlaceableObjectsLibrarySO placeableObjectLibrary;
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

        InitializeLibraryBar();
        LoadObjectsForCategory(PlaceableObjectCategories.Structure);
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
                Utils.SelectionRaiser.RaiseSelected(placeable);
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
