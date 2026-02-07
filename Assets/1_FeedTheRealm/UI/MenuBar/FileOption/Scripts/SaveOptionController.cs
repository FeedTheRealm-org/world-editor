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
            Instantiate(saveMenuPrefab);
        }
    }
}
