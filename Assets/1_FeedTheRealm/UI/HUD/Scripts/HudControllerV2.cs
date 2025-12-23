using System.Collections.Generic;
using UnityEngine;

public class HudControllerV2 : MonoBehaviour
{
    [SerializeField]
    private CreatorLibrary creatorLibrary;

    [SerializeField]
    private LibraryHudController libraryHudController;

    void Awake()
    {
        InitializeHUD();
    }

    private void InitializeHUD()
    {
        creatorLibrary.InitializeLibrary();
        List<WorldObjectController> worldObjects = creatorLibrary.GetObjects();
        libraryHudController.RenderObjectButtons(worldObjects);
    }
}


// TODO: implemnt this when menus are needed
// public class MenuSetup {
//     [SerializeField] public string MenuDisplay;
//     [SerializeField] public GameObject MenuObject;
//     [SerializeField] public Color MenuColor = Color.black;
// }
