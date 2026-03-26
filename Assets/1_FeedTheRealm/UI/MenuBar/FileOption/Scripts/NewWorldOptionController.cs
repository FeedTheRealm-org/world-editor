using System.Runtime.InteropServices;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace FeedTheRealm.UI.MenuBar
{
    public class NewWorldOptionController : MenuOption
    {
        [Inject]
        private WorldSelector worldSelector;

        [SerializeField]
        private SceneReference worldEditorScene;

        public override void Execute()
        {
            worldSelector.selectedWorld = null; // Clear selected world to start fresh
            SceneManager.LoadScene(worldEditorScene.SceneName);
        }
    }
}
