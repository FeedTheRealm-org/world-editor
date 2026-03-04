using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FeedTheRealm.UI.MenuBar
{
    public class ExitToMenuOptionController : MenuOption
    {
        [SerializeField]
        private SceneReference mainMenuScene;

        [SerializeField]
        private DataPersistenceManagerSO dataPersistenceManager;

        public override void Execute()
        {
            if (mainMenuScene != null)
            {
                SceneManager.LoadScene(mainMenuScene.SceneName);
                dataPersistenceManager.UnsetActiveWorld();
            }
            else
                Debug.LogError("ExitToMenuOptionController: MainMenuScene reference is not set!");
        }
    }
}
