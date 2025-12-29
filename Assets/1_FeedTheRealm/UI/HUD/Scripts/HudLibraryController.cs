using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HudLibraryController : MonoBehaviour
{
    [SerializeField]
    private CreatorLibraryController creatorLibrary;
    private ScrollView assetContainer;

    void Start()
    {
        creatorLibrary.Initialize();
        UIDocument hudVisualDocument = GetComponent<UIDocument>();
        assetContainer = hudVisualDocument.rootVisualElement.Q<ScrollView>("LibraryHUD");
        RenderObjectButtons();
    }

    private void RenderObjectButtons()
    {
        List<WorldObjectDefinition> worldObjects = creatorLibrary.GetObjects(
            WorldObjectCategories.Structure
        );
        foreach (var worldObject in worldObjects)
        {
            var assetButton = new Button() { text = worldObject.DisplayName };
            assetButton.clicked += () =>
            {
                Utils.WorldObjectSelectionEvents.RaiseObjectSelected(worldObject);
            };
            assetButton.AddToClassList("libraryListButtons");
            assetContainer.Add(assetButton);
        }
    }
}
