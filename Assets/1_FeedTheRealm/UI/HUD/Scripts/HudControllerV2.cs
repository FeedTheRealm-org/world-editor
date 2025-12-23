using UnityEngine;

public class HudControllerV2 : MonoBehaviour
{
    [SerializeField]
    private LibraryHudController libraryHudController;

    void Start()
    {
        InitializeHUD();
    }

    private void InitializeHUD()
    {
        libraryHudController.Initialize();
    }
}


// TODO: implemnt this when menus are needed
// public class MenuSetup {
//     [SerializeField] public string MenuDisplay;
//     [SerializeField] public GameObject MenuObject;
//     [SerializeField] public Color MenuColor = Color.black;
// }
