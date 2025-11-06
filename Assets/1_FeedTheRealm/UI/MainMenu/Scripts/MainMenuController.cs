using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenu : MonoBehaviour {

    public VisualElement ui;
    public Button _startButton;
    public Button _browseButton;
    public Button _quitButton;

    [SerializeField]
    private SceneReference newWorldScene;

    [SerializeField]
    private SceneReference loadWorldScene;
    [SerializeField]
    private DataPersistenceManagerSO dataPersistenceManager;

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
        // TODO: in the future, this should be a new scene that lets
        // makers set preset values for their new world
        dataPersistenceManager.NewWorld();
        SceneManager.LoadScene(newWorldScene.SceneName);
    }

    private void OnBrowseClicked() {
        SceneManager.LoadScene(loadWorldScene.SceneName);
    }

    private void OnQuitClicked() {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

}
