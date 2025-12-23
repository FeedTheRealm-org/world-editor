using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class LibraryHudController : MonoBehaviour
{
    private ScrollView assetContainer;

    void Awake()
    {
        var root = GetComponent<UIDocument>();
        assetContainer = root.rootVisualElement.Q<ScrollView>("LibraryHUD");
        RemoveTemplate();
    }

    public void RenderObjectButtons(List<WorldObjectController> worldObjects)
    {
        foreach (var worldObject in worldObjects)
        {
            var assetButton = new Button(() =>
            {
                Debug.Log($"Selected asset: {worldObject.name}");
            })
            {
                text = worldObject.DisplayName,
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
