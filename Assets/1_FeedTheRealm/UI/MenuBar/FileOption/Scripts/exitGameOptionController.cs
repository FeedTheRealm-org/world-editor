using FTR.UI;
using UnityEngine;

namespace FeedTheRealm.UI.MenuBar
{
    public class ExitGameOptionController : MenuOption
    {
        public override void Execute()
        {
#if !UNITY_EDITOR
            Application.Quit();
#endif
        }
    }
}
