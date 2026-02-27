using UnityEngine;

namespace FeedTheRealm.Core.WorldObjects.Provider
{
    [CreateAssetMenu(
        fileName = "UIObjectProvider",
        menuName = "Scriptable Objects/UIObjectProvider"
    )]
    public class UIObjectProvider : ScriptableObject
    {
        public GameObject menuBarGameObject;
        public GameObject editorBarGameObject;
        public GameObject placeableDisplayObject;
        public GameObject editorSettingsMenuObject;
    }
}
