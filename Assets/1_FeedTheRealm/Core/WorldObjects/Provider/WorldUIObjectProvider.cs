using UnityEngine;

namespace FeedTheRealm.Core.WorldObjects.Provider
{
    [CreateAssetMenu(
        fileName = "WorldUIObjectProvider",
        menuName = "Scriptable Objects/Providers/WorldUIObjectProvider"
    )]
    public class WorldUIObjectProvider : ScriptableObject
    {
        public GameObject menuBarGameObject;
        public GameObject editorBarGameObject;
        public GameObject placeableDisplayObject;
        public GameObject editorSettingsMenuObject;
        public GameObject logingMenuObject;
        public GameObject signUpMenuObject;
        public GameObject verifyCodeMenuObject;
    }
}
