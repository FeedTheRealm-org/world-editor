using System;
using System.Collections.Generic;
using API;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.EditorBar.ElementOption.CharacterEditor
{
    internal sealed class CharacterEditorPreviewRenderer : IDisposable
    {
        private readonly GameObject characterEditorPrefab;

        private GameObject previewInstance;
        private CharacterEditController previewController;
        private Camera previewCamera;

        public CharacterEditorPreviewRenderer(GameObject characterEditorPrefab)
        {
            this.characterEditorPrefab = characterEditorPrefab;
        }

        public void Refresh(Image targetImage, CharacterInfoResponse characterInfo)
        {
            if (targetImage == null)
                return;

            if (!EnsurePreviewInstance())
            {
                targetImage.image = null;
                return;
            }

            HideEditorUi();

            targetImage.image = previewCamera.targetTexture;
            targetImage.scaleMode = ScaleMode.ScaleToFit;

            var safeInfo = CloneCharacterInfo(characterInfo);
            previewController.SetupWithCharacterInfo(safeInfo, null);
        }

        public void SetVisible(bool isVisible)
        {
            if (previewInstance == null)
                return;

            previewInstance.SetActive(isVisible);
            if (isVisible)
            {
                HideEditorUi();
            }
        }

        public void Dispose()
        {
            if (previewInstance != null)
            {
                UnityEngine.Object.Destroy(previewInstance);
                previewInstance = null;
            }

            previewController = null;
            previewCamera = null;
        }

        private bool EnsurePreviewInstance()
        {
            if (previewInstance != null && previewController != null && previewCamera != null)
                return true;

            if (characterEditorPrefab == null)
                return false;

            previewInstance = UnityEngine.Object.Instantiate(characterEditorPrefab);
            previewInstance.name = $"{characterEditorPrefab.name}_Preview";

            previewController = previewInstance.GetComponentInChildren<CharacterEditController>(
                true
            );
            previewCamera = previewInstance.GetComponentInChildren<Camera>(true);

            if (previewController == null || previewCamera == null)
            {
                Dispose();
                return false;
            }

            HideEditorUi();
            return true;
        }

        private void HideEditorUi()
        {
            if (previewController == null)
                return;

            var document = previewController.GetComponent<UIDocument>();
            if (document == null)
                return;

            var root = document.rootVisualElement;
            if (root == null)
                return;

            root.style.visibility = Visibility.Hidden;
            root.style.opacity = 0f;
            root.pickingMode = PickingMode.Ignore;

            // The character editor prefab has its own uGUI preview surface; disable it so
            // only the menu's target image displays the render texture.
            if (previewInstance == null)
                return;

            var canvases = previewInstance.GetComponentsInChildren<Canvas>(true);
            foreach (var canvas in canvases)
            {
                canvas.enabled = false;
            }
        }

        private static CharacterInfoResponse CloneCharacterInfo(CharacterInfoResponse source)
        {
            var safeSource = source ?? new CharacterInfoResponse();
            return new CharacterInfoResponse
            {
                character_name = safeSource.character_name ?? string.Empty,
                character_bio = safeSource.character_bio ?? string.Empty,
                category_sprites =
                    safeSource.category_sprites != null
                        ? new Dictionary<string, string>(safeSource.category_sprites)
                        : new Dictionary<string, string>(),
            };
        }
    }
}
