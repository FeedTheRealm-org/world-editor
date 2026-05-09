using System;
using System.IO;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.Repository;
using FeedTheRealm.UI.Common;
using FTR.Core.Common.Config;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;
using VContainer;

namespace FeedTheRealm.UI.MenuBar.EditOption.SettingsMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class CustomModelMenu : MenuController
    {
        [Inject]
        private ModelsRepository modelsRepository;

        [Inject]
        private RefreshPlaceableLibraryEvent refreshPlaceableLibraryEvent;

        private Button selectModelButton;
        private Button saveModelButton;
        private Button closeButton;
        private Label modelNameLabel;
        private Toggle hasCollidersToggle;

        private string pendingModelPath;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            selectModelButton = root.Q<Button>("SelectModel");
            saveModelButton = root.Q<Button>("SaveModel");
            closeButton = root.Q<Button>("Close");
            modelNameLabel = root.Q<Label>("ModelName");
            hasCollidersToggle = root.Q<Toggle>();

            saveModelButton.text = "Import Model";
            saveModelButton.SetEnabled(false);

            selectModelButton.clicked += OnSelectModelClicked;
            saveModelButton.clicked += OnSaveModelClicked;
            closeButton.clicked += CloseMenu;
        }

        void OnDisable()
        {
            selectModelButton.clicked -= OnSelectModelClicked;
            saveModelButton.clicked -= OnSaveModelClicked;
            closeButton.clicked -= CloseMenu;
        }

        private void OnSelectModelClicked()
        {
            CustomFileBrowser.ShowFilePickerDialog(
                onSuccess: paths =>
                {
                    if (paths == null || paths.Length == 0)
                        return;

                    if (!paths[0].EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
                    {
                        modelNameLabel.text = "Error: only .glb files are supported.";
                        saveModelButton.SetEnabled(false);
                        return;
                    }

                    pendingModelPath = paths[0];
                    modelNameLabel.text = Path.GetFileName(pendingModelPath);
                    saveModelButton.SetEnabled(true);
                },
                onCancel: () => { },
                extensions: ".glb"
            );
        }

        private void OnSaveModelClicked()
        {
            if (string.IsNullOrEmpty(pendingModelPath) || !File.Exists(pendingModelPath))
            {
                modelNameLabel.text = "Error: file not found.";
                return;
            }

            try
            {
                string fileName = Path.GetFileName(pendingModelPath);
                string modelName = Path.GetFileNameWithoutExtension(pendingModelPath);

                var structureData = new StructureData(
                    id: Guid.NewGuid().ToString(),
                    structureName: modelName,
                    size: Vector3.one,
                    rotation: Vector3.zero,
                    fileName: fileName,
                    hasColliders: hasCollidersToggle.value
                );

                modelsRepository.AddModel(structureData, pendingModelPath);
                refreshPlaceableLibraryEvent.Raise();

                saveModelButton.SetEnabled(false);
                ToastNotification.Show(
                    $"'{modelName}' imported successfully!",
                    "success",
                    Color.green
                );
                CloseMenu();
            }
            catch (Exception ex)
            {
                modelNameLabel.text = $"Error: {ex.Message}";
                Debug.LogError($"[CustomModelMenu] Failed to import model: {ex.Message}");
                ToastNotification.Show($"Failed to import model: {ex.Message}", "error", Color.red);
            }
        }
    }
}
