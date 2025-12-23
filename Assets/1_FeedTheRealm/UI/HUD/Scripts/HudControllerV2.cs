using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HudControllerV2 : MonoBehaviour {
    [SerializeField] private List<MenuSetup> menuOptions;
    [SerializeField] private AssetSelectorController assetSelectorController;

    private UIDocument hudDocument;


    void Awake() {
        hudDocument = GetComponent<UIDocument>();
        RemoveTemplates();
    }



    private void RemoveTemplates() {
        var root = hudDocument.rootVisualElement;

        // Remove template buttons from MenuOptions
        var menuOptionsContainer = root.Q<VisualElement>("MenuOptions");
        if (menuOptionsContainer != null) {
            var buttons = menuOptionsContainer.Query<Button>().ToList();
            foreach (var button in buttons) {
                button.RemoveFromHierarchy();
            }
        }
        // Remove template buttons from AssetsList
        var assetsList = root.Q<ScrollView>("AssetsList");
        if (assetsList != null) {
            var buttons = assetsList.Query<Button>().ToList();
            foreach (var button in buttons) {
                button.RemoveFromHierarchy();
            }
        }
    }

}



public class MenuSetup {
    [SerializeField] public string MenuDisplay;
    [SerializeField] public GameObject MenuObject;
    [SerializeField] public Color MenuColor = Color.black;
}
