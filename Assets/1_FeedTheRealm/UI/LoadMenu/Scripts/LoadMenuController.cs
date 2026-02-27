using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VContainer;

[RequireComponent(typeof(UIDocument))]
public class LoadMenu : MonoBehaviour
{
    public VisualElement ui;
    public Button _mainMenuButton;
    public ListView _listView;

    [SerializeField]
    private SceneReference mainMenuScene;

    [SerializeField]
    private SceneReference gameScene;

    [Inject]
    private DataPersistenceManagerSO dataPersistenceManager;

    private List<string> loadedWorlds;

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        // Get UI elements
        _mainMenuButton = ui.Q<Button>("MainMenu");
        _listView = ui.Q<ListView>();

        loadedWorlds = dataPersistenceManager.ListAllWorlds();

        _mainMenuButton.clicked += OnMainMenuClicked;

        // Configure ListView
        _listView.makeItem = () =>
        {
            // Create a button styled with the USS class
            var button = new Button();
            button.AddToClassList("load-world-button");
            return button;
        };

        _listView.bindItem = (element, index) =>
        {
            var button = element as Button;
            button.text = PrettifyName(loadedWorlds[index]);

            button.clicked -= () => { };
            button.clicked += () =>
            {
                OnLoadWorldClicked(loadedWorlds[index]);
            };
        };

        _listView.itemsSource = loadedWorlds;
        _listView.selectionType = SelectionType.None;
        _listView.reorderable = false;
    }

    private void OnMainMenuClicked()
    {
        SceneManager.LoadScene(mainMenuScene.SceneName);
    }

    private void OnLoadWorldClicked(string worldName)
    {
        dataPersistenceManager.SetActiveWorld(worldName);
        SceneManager.LoadScene(gameScene.SceneName);
    }

    private string PrettifyName(string name)
    {
        return string.Join(
            " ",
            name.Split('_').Select(word => char.ToUpper(word[0]) + word.Substring(1))
        );
    }
}
