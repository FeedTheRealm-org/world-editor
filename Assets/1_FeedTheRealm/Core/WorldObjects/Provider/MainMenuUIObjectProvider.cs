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
        public GameObject menuBarGameObject;
        public GameObject logingMenuObject;
        public GameObject signUpMenuObject;
        public GameObject verifyCodeMenuObject;
        public GameObject loginBackgroundObject;
    }
}
