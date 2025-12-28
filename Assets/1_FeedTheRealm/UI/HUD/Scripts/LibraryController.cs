using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HudLibraryController : MonoBehaviour
{
    [SerializeField]
    private CreatorLibraryController creatorLibrary;

    [SerializeField]
    private UIDocument HudLibraryDocument;
    private ScrollView assetContainer;

    public void Initialize()
    {
        creatorLibrary.Initialize();
        assetContainer = HudLibraryDocument.rootVisualElement.Q<ScrollView>("LibraryHUD");
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
                Utils.WorldObjectSelectionEvents.RaiseObjectSelected(worldObject);
            };
            assetButton.AddToClassList("assetListButtons");
            assetContainer.Add(assetButton);
        }
    }
}
