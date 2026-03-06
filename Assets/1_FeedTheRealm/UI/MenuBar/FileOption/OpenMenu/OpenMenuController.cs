using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.MenuBar.FileOption.OpenMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class OpenMenuController : MenuController
    {
        [SerializeField]
        private DataPersistenceManagerSO dataPersistenceManager;

        [SerializeField]
        private SceneReference gameScene;

        private Button closeButton;
        private ListView worldsListView;
        private VisualElement root;
        private List<string> loadedWorlds;

        void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;
            closeButton = root.Q<Button>("Close");
            worldsListView = root.Q<ListView>("WorldsList");

            loadedWorlds = dataPersistenceManager.ListAllWorlds();

            closeButton.clicked += CloseMenu;

            // Configure ListView
            worldsListView.makeItem = () =>
            {
                var button = new Button();
                button.AddToClassList("open-world-button");
                return button;
            };

            worldsListView.bindItem = (element, index) =>
            {
                var button = element as Button;
                button.text = PrettifyName(loadedWorlds[index]);

                button.clicked -= () => { };
                button.clicked += () =>
                {
                    OnLoadWorldClicked(loadedWorlds[index]);
                };
            };

            worldsListView.itemsSource = loadedWorlds;
            worldsListView.selectionType = SelectionType.None;
            worldsListView.reorderable = false;
        }

        private void OnDisable()
        {
            closeButton.clicked -= CloseMenu;
        }

        private void OnLoadWorldClicked(string worldName)
        {
            dataPersistenceManager.SetActiveWorld(worldName);
            SceneManager.LoadScene(gameScene.SceneName);
            CloseMenu();
        }

        private string PrettifyName(string name)
        {
            return string.Join(
                " ",
                name.Split('_').Select(word => char.ToUpper(word[0]) + word.Substring(1))
            );
        }
    }
}
