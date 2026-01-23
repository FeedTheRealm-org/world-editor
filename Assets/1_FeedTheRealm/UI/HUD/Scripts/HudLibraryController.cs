using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HudLibraryController : MonoBehaviour
{
    [SerializeField]
    private PlaceableObjectsLibrarySO placeableObjectLibrary;
    private ScrollView libraryHUD;

    private DropdownField libraryOptions;

    void Start()
    {
        // TODO: review a better way to initialize the library it shouldnt have to be done here
        placeableObjectLibrary.Initialize();
        UIDocument hudVisualDocument = GetComponent<UIDocument>();
        libraryHUD = hudVisualDocument.rootVisualElement.Q<ScrollView>("LibraryHUD");
        RenderDropDown();
        RenderObjectButtons(PlaceableObjectCategories.Structure);
    }

    private void RenderDropDown()
    {
        libraryOptions = GetComponent<UIDocument>()
            .rootVisualElement.Q<DropdownField>("LibraryOptions");

        libraryOptions.choices = new List<string>();
        foreach (var category in System.Enum.GetValues(typeof(PlaceableObjectCategories)))
        {
            libraryOptions.choices.Add(category.ToString());
        }

        libraryOptions.RegisterValueChangedCallback(evt =>
        {
            Debug.Log("Category changed to: " + evt.newValue);
            RenderObjectButtons(
                (PlaceableObjectCategories)
                    System.Enum.Parse(typeof(PlaceableObjectCategories), evt.newValue)
            );
        });
    }

    private void RenderObjectButtons(PlaceableObjectCategories category)
    {
        libraryHUD.Clear();
        List<IPlaceable> worldObjects = placeableObjectLibrary.GetObjects(category);
        foreach (var worldObject in worldObjects)
        {
            var assetButton = new Button() { text = worldObject.DisplayName };
            assetButton.clicked += () =>
            {
                Utils.SelectionRaiser.RaiseSelected(worldObject);
            };
            assetButton.AddToClassList("libraryListButtons");
            libraryHUD.Add(assetButton);
        }
    }
}
