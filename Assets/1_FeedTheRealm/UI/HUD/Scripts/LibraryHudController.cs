using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class LibraryHudController : MonoBehaviour
{
    [SerializeField]
    private CreatorLibraryController creatorLibrary;
    private ScrollView assetContainer;

    public void Initialize()
    {
        creatorLibrary.Initialize();
        var root = GetComponent<UIDocument>();
        assetContainer = root.rootVisualElement.Q<ScrollView>("LibraryHUD");
        RemoveTemplate();
        RenderObjectButtons();
    }

    private void RenderObjectButtons()
    {
        List<WorldObjectReference> worldObjects = creatorLibrary.GetObjects();
        foreach (var worldObject in worldObjects)
        {
            var assetButton = new Button() { text = worldObject.DisplayName };
            assetButton.clicked += () =>
            {
                WorldObjectSelectionEvents.RaiseObjectSelected(worldObject);
            };
            assetButton.AddToClassList("assetListButtons");
            assetContainer.Add(assetButton);
        }
    }

    private void RemoveTemplate()
    {
        var buttons = assetContainer.Query<Button>().ToList();
        foreach (var button in buttons)
        {
            button.RemoveFromHierarchy();
        }
    }
}
