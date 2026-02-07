using FeedTheRealm.UI.Common;
using UnityEngine;

namespace FeedTheRealm.UI.MenuBar
{
    public class PublishOptionController : MenuOption
    {
        [SerializeField]
        private GameObject publishMenuPrefab;

        public override void Execute()
        {
            Instantiate(publishMenuPrefab);
        }
    }
}
