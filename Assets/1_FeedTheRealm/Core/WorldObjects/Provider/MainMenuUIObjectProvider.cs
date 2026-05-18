using UnityEngine;

namespace FeedTheRealm.Core.WorldObjects.Provider
{
    [CreateAssetMenu(
        fileName = "MainMenuUIObjectProvider",
        menuName = "Scriptable Objects/Providers/MainMenuUIObjectProvider"
    )]
    public class MainMenuUIObjectProvider : ScriptableObject
    {
        public GameObject mainMenuGameObject;
    }
}
