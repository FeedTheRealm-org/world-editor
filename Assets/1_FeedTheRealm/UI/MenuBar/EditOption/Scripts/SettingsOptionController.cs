using FeedTheRealm.UI.Common;
using UnityEngine;

public class SettingsOptionController : MenuOption
{
    [SerializeField]
    GameObject settingsMenuPrefab;

    public override void Execute()
    {
        Instantiate(settingsMenuPrefab);
    }
}
