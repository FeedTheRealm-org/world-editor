using FeedTheRealm.Core.DataPersistence;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.MainMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenu : MonoBehaviour
    {
        public VisualElement ui;
        public Button _startButton;
        public Button _browseButton;
        public Button _quitButton;

        [Inject]
        private WorldSelector worldSelector;

        [SerializeField]
        private SceneReference newWorldScene;

        [SerializeField]
        private SceneReference loadWorldScene;

        [SerializeField]
        private DataPersistenceManager dataPersistenceManager;

        private void Awake()
        {
            Application.runInBackground = true;
            ui = GetComponent<UIDocument>().rootVisualElement;
        }

        private void OnEnable()
        {
            _startButton = ui.Q<Button>("Start");
            _startButton.clicked += OnStartClicked;

            _quitButton = ui.Q<Button>("Quit");
            _quitButton.clicked += OnQuitClicked;
        }

        private void OnStartClicked()
        {
            // TODO: in the future, this should be a new scene that lets
            // makers set preset values for their new world
            //dataPersistenceManager.NewWorld();
            worldSelector.ClearSelection();
            SceneManager.LoadScene(newWorldScene.SceneName);
        }

        private void OnQuitClicked()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
