using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
public class MainMenu : MonoBehaviour {

    public VisualElement ui;
    public Button _startButton;
    public Button _browseButton;
    public Button _quitButton;

    [SerializeField] Maker player;


    private void Awake() {
        player.gameObject.SetActive(false);
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
        Debug.Log("Start Button Clicked");
        gameObject.SetActive(false);

        // Re-lock the cursor so mouse look works again
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        player.gameObject.SetActive(true);

    }

    private void OnBrowseClicked() {
        Debug.Log("Browse Button Clicked");
    }

    private void OnQuitClicked() {
        Application.Quit();
        Debug.Log("Quit Button Clicked");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

}
