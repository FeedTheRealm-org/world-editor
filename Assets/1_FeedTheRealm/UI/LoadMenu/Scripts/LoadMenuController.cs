using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class LoadMenu : MonoBehaviour {

    public VisualElement ui;
    public Button _mainMenuButton;

    [SerializeField]
    private SceneReference mainMenuScene;


    private void Awake() {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable() {

        _mainMenuButton = ui.Q<Button>("MainMenu");
        _mainMenuButton.clicked += OnMainMenuClicked;
    }

    private void OnMainMenuClicked() {
        SceneManager.LoadScene(mainMenuScene.SceneName);
    }

}
