using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
public class MainMenu : MonoBehaviour {

    public VisualElement ui;
    public Button _startButton;
    public Button _browseButton;
    public Button _quitButton;

    [SerializeField]
    private SceneReference gameScene;


    private void Awake() {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable() {
        _startButton = ui.Q<Button>("Start");
        _startButton.clicked += OnStartClicked;

        _browseButton = ui.Q<Button>("Browse");
        _browseButton.clicked += OnBrowseClicked;

        _quitButton = ui.Q<Button>("Quit");
        _quitButton.clicked += OnQuitClicked;
    }

    private void OnStartClicked() {
        SceneManager.LoadScene(gameScene.SceneName);
    }

    private void OnBrowseClicked() {
        Debug.Log("Browse Button Clicked");
    }

    private void OnQuitClicked() {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

}
