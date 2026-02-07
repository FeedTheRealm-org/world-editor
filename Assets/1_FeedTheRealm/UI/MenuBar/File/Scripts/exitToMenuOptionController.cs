using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FeedTheRealm.UI.MenuBar
{
    public class ExitToMenuOptionController : MenuOption
    {
        [SerializeField]
        SceneReference mainMenuScene;

        public override void Execute()
        {
            if (mainMenuScene != null)
                SceneManager.LoadScene(mainMenuScene.SceneName);
            else
                Debug.LogError("ExitToMenuOptionController: MainMenuScene reference is not set!");
        }
    }
}
