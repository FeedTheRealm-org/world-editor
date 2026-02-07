using FeedTheRealm.UI.Common;
using UnityEngine;

namespace FeedTheRealm.UI.MenuBar
{
    public class SaveOptionController : MenuOption
    {
        [SerializeField]
        private GameObject saveMenuPrefab;

        public override void Execute()
        {
            var saveMenuInstance = Instantiate(saveMenuPrefab);
            saveMenuInstance.SetActive(true);
        }
    }
}
