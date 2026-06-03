using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.WorldLoader;
using FTR.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.MenuBar.FileOption.OpenMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class OpenMenuController : MenuController
    {
        [SerializeField]
        private SceneReference editorScene;

        [Inject]
        private WorldSelector worldSelector;

        [Inject]
        private ZoneLoader zoneLoader;

        [Inject]
        private CreatablesLoader creatablesLoader;

        [Inject]
        private DataPersistenceManager dataPersistenceManager;

        [Inject]
        private RefreshZonesEvent refreshZonesEvent;

        [Inject]
        private WorldPrefabProvider prefabProvider;

        private Button closeButton;
        private ScrollView worldsListView;
        private VisualElement root;
        private List<string> loadedWorlds;

        void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;
            closeButton = root.Q<Button>("Close");
            worldsListView = root.Q<ScrollView>("WorldsList");

            closeButton.clicked += CloseMenu;

            loadedWorlds = dataPersistenceManager.ListAllWorlds();
            PopulateWorldsList();
        }

        private void PopulateWorldsList()
        {
            worldsListView.Clear();

            foreach (var world in loadedWorlds)
            {
                var capturedWorld = world;
                var button = new Button();
                button.text = PrettifyName(capturedWorld);
                button.AddToClassList("menu-world-option-button");
                button.style.alignSelf = Align.Stretch;

                button.clicked += async () => await OnLoadWorldClicked(capturedWorld);

                worldsListView.Add(button);
            }
        }

        private void OnDisable()
        {
            closeButton.clicked -= CloseMenu;
        }

        private async UniTask OnLoadWorldClicked(string worldName)
        {
            if (
                !string.IsNullOrEmpty(worldSelector.selectedWorld)
                && worldSelector.selectedWorld != worldName
            )
            {
                var confirmPopup = Instantiate(prefabProvider.confirmPopup);
                var dialogController = confirmPopup.GetComponent<ConfirmPopupController>();
                dialogController.Show(
                    title: "Open World",
                    question: "Are you sure you want to open another world? Any unsaved changes in your current world will be lost.",
                    onConfirm: async () => await ExecuteLoadWorld(worldName),
                    onCancel: () => { }
                );
            }
            else
            {
                await ExecuteLoadWorld(worldName);
            }
        }

        private async UniTask ExecuteLoadWorld(string worldName)
        {
            worldSelector.selectedWorld = worldName;
            worldSelector.selectedZoneId =
                dataPersistenceManager.GetWorldData(worldName)?.startingZone ?? 1;
            worldSelector.selectedWorldId = dataPersistenceManager.GetCurrentWorldId(worldName);

            if (SceneManager.GetActiveScene().name == editorScene.SceneName)
            {
                // If we're already in the game scene,
                // we can just load the new world data without reloading the scene
                await zoneLoader.Load();
                await creatablesLoader.Load();
                refreshZonesEvent.Raise();
                CloseMenu();
            }
            else
                SceneManager.LoadScene(editorScene.SceneName);
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
