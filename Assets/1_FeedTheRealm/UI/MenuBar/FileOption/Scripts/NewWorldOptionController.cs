using System;
using System.Runtime.InteropServices;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.UIEvents;
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

        public override async void Execute()
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
                    $"[NewWorldOptionController] Error executing: {ex.Message}",
                    Logging.LogType.Error
                );
            }
        }
    }
}
