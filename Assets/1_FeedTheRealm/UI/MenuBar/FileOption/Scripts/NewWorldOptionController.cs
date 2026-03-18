using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FeedTheRealm.UI.MenuBar
{
    public class NewWorldOptionController : MenuOption
    {
        [SerializeField]
        private DataPersistenceManager dataPersistenceManager;

        [SerializeField]
        private SceneReference worldEditorScene;

        public override void Execute()
        {
            dataPersistenceManager.NewWorld();
            SceneManager.LoadScene(worldEditorScene.SceneName);
        }
    }
}
