using System;
using FeedTheRealm.Core.DataPersistence;
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
        private WorldSelector worldSelector;

        [Inject]
        private ZoneLoader zoneLoader;

        [Inject]
        private CreatablesLoader creatablesLoader;

        public override async void Execute()
        {
            try
            {
                Debug.Log(
                    $"[NewWorldOptionController] world selector: {worldSelector != null}, zone loader: {zoneLoader != null}, creatables loader: {creatablesLoader != null}"
                );

                worldSelector.selectedWorld = null;
                worldSelector.selectedZoneId = 1;

                if (SceneManager.GetActiveScene().name == editorScene.SceneName)
                {
                    await zoneLoader.Load();
                    await creatablesLoader.Load();
                }
                else
                    SceneManager.LoadScene(editorScene.SceneName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NewWorldOptionController] Error executing: {ex.Message}");
            }
        }
    }
}
