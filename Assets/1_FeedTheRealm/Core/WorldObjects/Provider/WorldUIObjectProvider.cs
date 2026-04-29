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
        public GameObject loginMenuObject;
        public GameObject signUpMenuObject;
        public GameObject verifyCodeMenuObject;
        public GameObject subscriptionMenuObject;
        public GameObject transactionsMenuObject;

        [Header("Placeable Editor UI")]
        public GameObject placeableEditor;
        public GameObject structureEditObject;
        public GameObject FriendlyNpcSpawnerEditObject;
        public GameObject AggresiveNpcSpawnerEditObject;
        public GameObject PortalEditObject;
    }
}
