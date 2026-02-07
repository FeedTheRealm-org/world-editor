using System;
using FeedTheRealm.UI.Common;
using UnityEngine;

namespace FeedTheRealm.UI.MenuBar
{
    public class OpenOptionController : MenuOption
    {
        [SerializeField]
        GameObject openMenuPrefab;

        public override void Execute()
        {
            Instantiate(openMenuPrefab);
        }
    }
}
