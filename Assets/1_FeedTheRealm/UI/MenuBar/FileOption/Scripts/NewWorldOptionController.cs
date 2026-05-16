using System;
using System.Threading.Tasks;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.WorldLoader;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace FeedTheRealm.UI.MenuBar
{
    public class NewWorldOptionController : MenuOption
    {
        [SerializeField]
        private SceneReference editorScene;

        [Inject]
        private Logging.Logger logger;

        [Inject]
        private WorldSelector worldSelector;

        [Inject]
        private DataPersistenceManager dataPersistenceManager;

        [Inject]
        private ZoneLoader zoneLoader;

        [Inject]
        private CreatablesLoader creatablesLoader;

        [Inject]
        private RefreshZonesEvent refreshZonesEvent;

        [Inject]
        private WorldPrefabProvider prefabProvider;

        public override async void Execute()
        {
            try
            {
                if (!string.IsNullOrEmpty(worldSelector.selectedWorld))
                {
                    var confirmPopup = Instantiate(prefabProvider.confirmPopup);
                    var dialogController = confirmPopup.GetComponent<ConfirmPopupController>();
                    dialogController.Show(
                        title: "New World",
                        question: "Are you sure you want to create a new world? Any unsaved changes in your current world will be lost.",
                        onConfirm: async () => await ExecuteNewWorld(),
                        onCancel: () => { }
                    );
                }
                else
                {
                    await ExecuteNewWorld();
                }
            }
            catch (Exception ex)
            {
                logger.Log(
                    $"[NewWorldOptionController] Error executing: {ex.Message}",
                    Logging.LogType.Error
                );
            }
        }

        private async Task ExecuteNewWorld()
        {
            try
            {
                worldSelector.ClearSelection();

                if (SceneManager.GetActiveScene().name == editorScene.SceneName)
                {
                    refreshZonesEvent.Raise();
                    await creatablesLoader.Load();
                    await zoneLoader.Load();
                }
                else
                    SceneManager.LoadScene(editorScene.SceneName);
            }
            catch (Exception ex)
            {
                logger.Log(
                    $"[NewWorldOptionController] Error executing New World: {ex.Message}",
                    Logging.LogType.Error
                );
            }
        }
    }
}
