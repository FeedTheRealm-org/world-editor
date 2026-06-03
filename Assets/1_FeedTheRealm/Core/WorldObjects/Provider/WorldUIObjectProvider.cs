using UnityEngine;

namespace FeedTheRealm.Core.WorldObjects.Provider
{
    [CreateAssetMenu(
        fileName = "WorldUIObjectProvider",
        menuName = "Scriptable Objects/Providers/WorldUIObjectProvider"
    )]
    public class WorldUIObjectProvider : ScriptableObject
    {
        [Header("HUD Elements")]
        public GameObject menuBarGameObject;
        public GameObject editorBarGameObject;
        public GameObject editorStateDisplayObject;
        public GameObject placeableDisplayObject;
        public GameObject editorSettingsMenuObject;
        public GameObject subscriptionMenuObject;
        public GameObject transactionsMenuObject;

        [Header("Placeable Editor UI")]
        public GameObject placeableEditor;
        public GameObject structureEditObject;
        public GameObject FriendlyNpcSpawnerEditObject;
        public GameObject AggresiveNpcSpawnerEditObject;
        public GameObject PortalEditObject;
        public GameObject ChestEditObject;
        public GameObject PlayerSpawnerEditObject;
    }
}
