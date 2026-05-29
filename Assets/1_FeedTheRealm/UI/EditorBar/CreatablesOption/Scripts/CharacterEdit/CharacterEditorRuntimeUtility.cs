using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.EditorBar.ElementOption.CharacterEditor
{
    internal static class CharacterEditorRuntimeUtility
    {
        public static GameObject ResolveCharacterEditorPrefab(
            Component host,
            GameObject characterEditorPrefab
        )
        {
            if (characterEditorPrefab != null)
                return characterEditorPrefab;

            var embeddedEditor = host.GetComponentInChildren<CharacterEditController>(true);
            if (embeddedEditor == null)
                return null;

            return embeddedEditor.transform.parent != null
                ? embeddedEditor.transform.parent.gameObject
                : embeddedEditor.gameObject;
        }

        public static void HideEmbeddedCharacterEditors(Component host)
        {
            var embeddedEditors = host.GetComponentsInChildren<CharacterEditController>(true);
            foreach (var embeddedEditor in embeddedEditors)
            {
                var root =
                    embeddedEditor.transform.parent != null
                        ? embeddedEditor.transform.parent.gameObject
                        : embeddedEditor.gameObject;

                root.SetActive(false);
            }
        }

        public static bool TryInstantiateCharacterEditor(
            Component host,
            GameObject characterEditorPrefab,
            out GameObject editorInstance,
            out CharacterEditController editorController,
            IObjectResolver resolver
        )
        {
            editorInstance = null;
            editorController = null;

            if (characterEditorPrefab == null)
            {
                Debug.LogError("Character editor prefab is not assigned.", host);
                return false;
            }

            editorInstance = resolver.Instantiate(characterEditorPrefab);
            editorInstance.name = $"{characterEditorPrefab.name}_Runtime";
            editorInstance.transform.SetParent(null, false);
            editorController = editorInstance.GetComponentInChildren<CharacterEditController>(true);

            if (editorController != null)
            {
                editorInstance.SetActive(false);
                return true;
            }

            Debug.LogError(
                "CharacterEditController component was not found on instantiated character editor prefab.",
                host
            );

            Object.Destroy(editorInstance);
            editorInstance = null;
            return false;
        }

        public static void DestroyCharacterEditorInstance(
            ref GameObject editorInstance,
            ref CharacterEditController editorController
        )
        {
            if (editorInstance != null)
            {
                Object.Destroy(editorInstance);
                editorInstance = null;
            }

            editorController = null;
        }
    }
}
