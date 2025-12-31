using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HudLibraryController : MonoBehaviour
{
    [SerializeField]
    private CreatorLibrarySO creatorLibrary;
    private ScrollView libraryHUD;

    private DropdownField libraryOptions;

    void Start()
    {
        creatorLibrary.Initialize();
        UIDocument hudVisualDocument = GetComponent<UIDocument>();
        libraryHUD = hudVisualDocument.rootVisualElement.Q<ScrollView>("LibraryHUD");
        RenderDropDown();
        RenderObjectButtons(WorldObjectCategories.Structure);
    }

    private void RenderDropDown()
    {
        libraryOptions = GetComponent<UIDocument>()
            .rootVisualElement.Q<DropdownField>("LibraryOptions");

        libraryOptions.choices = new List<string>();
        foreach (var category in System.Enum.GetValues(typeof(WorldObjectCategories)))
        {
            libraryOptions.choices.Add(category.ToString());
        }

        libraryOptions.RegisterValueChangedCallback(evt =>
        {
            Debug.Log("Category changed to: " + evt.newValue);
            RenderObjectButtons(
                (WorldObjectCategories)
                    System.Enum.Parse(typeof(WorldObjectCategories), evt.newValue)
            );
        });
    }

    private void RenderObjectButtons(WorldObjectCategories category)
    {
        libraryHUD.Clear();
        List<IPlaceable> worldObjects = creatorLibrary.GetObjects(category);
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
