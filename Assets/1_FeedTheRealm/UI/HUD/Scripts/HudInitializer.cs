using System.Threading.Tasks;
using UnityEngine;

public class HudInitializer : MonoBehaviour
{
    [SerializeField]
    private LibraryController libraryController;

    void Start()
    {
        InitializeHUD();
    }

    private void InitializeHUD()
    {
        libraryController.Initialize();
    }
}


// TODO: implemnt this when menus are needed
// public class MenuSetup {
//     [SerializeField] public string MenuDisplay;
//     [SerializeField] public GameObject MenuObject;
//     [SerializeField] public Color MenuColor = Color.black;
// }
