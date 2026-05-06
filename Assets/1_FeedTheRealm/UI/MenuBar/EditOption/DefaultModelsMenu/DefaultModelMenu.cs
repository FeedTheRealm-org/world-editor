using System;
using System.IO;
using System.Threading.Tasks;
using API;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.Repository;
using FeedTheRealm.UI.Common;
using FTR.Core.Common.Config;
using FTR.Core.Enums;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.MenuBar.EditOption.SettingsMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class DefaultModelMenu : MenuController
    {
        [Inject]
        private ModelsRepository modelsRepository;

        [Inject]
        private ModelService modelService;

        [Inject]
        private Session.Session session;

        [Inject]
        private RefreshPlaceableLibraryEvent refreshPlaceableLibraryEvent;

        [Inject]
        private Config config;
        private Button downloadButton;
        private Button closeButton;
        private Label statusLabel;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            downloadButton = root.Q<Button>("DownloadModels");
            closeButton = root.Q<Button>("Close");
            statusLabel = root.Q<Label>("Status");

            downloadButton.clicked += OnDownloadClicked;
            closeButton.clicked += CloseMenu;
        }

        void OnDisable()
        {
            downloadButton.clicked -= OnDownloadClicked;
            closeButton.clicked -= CloseMenu;
        }

        private async void OnDownloadClicked()
        {
            downloadButton.SetEnabled(false);
            statusLabel.text = "Fetching default models list...";

            try
            {
                await DownloadDefaultModels();
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[DefaultModelMenu] Failed to download default models: {ex.Message}"
                );
                ToastNotification.Show(
                    $"Failed to download default models: {ex.Message}",
                    "error",
                    Color.red
                );
                statusLabel.text = $"Error: {ex.Message}";
            }
            finally
            {
                downloadButton.SetEnabled(true);
            }
        }

        private async Task DownloadDefaultModels()
        {
            var modelsInfo = await modelService.ListDefaultModels();

            int total = modelsInfo.Count;
            int current = 0;

            foreach (var kvp in modelsInfo)
            {
                var modelInfo = kvp.Value;
                string fileName = Path.GetFileName(modelInfo.url);
                string modelName = Path.GetFileNameWithoutExtension(modelInfo.url);

                current++;
                statusLabel.text = $"Downloading {current}/{total}: {fileName}";

                string tempPath = await modelService.DownloadModel(modelInfo);
                if (string.IsNullOrEmpty(tempPath))
                {
                    Debug.LogWarning($"[DefaultModelMenu] Skipping {fileName} — download failed.");
                    continue;
                }

                try
                {
                    bool hasColliders = !fileName.StartsWith(
                        ModelFilePrefixes.NoCollider,
                        StringComparison.OrdinalIgnoreCase
                    );
                    var structureData = new StructureData(
                        id: modelInfo.model_id,
                        structureName: ModelFilePrefixes.StripPrefix(modelName),
                        size: Vector3.one,
                        rotation: Vector3.zero,
                        fileName: ModelFilePrefixes.StripPrefix(fileName),
                        hasColliders: hasColliders,
                        isDefault: true
                    );
                    modelsRepository.AddModel(structureData, tempPath);
                    if (
                        fileName.StartsWith(
                            ModelFilePrefixes.DefaultChestClosed,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                        config.defaultClosedChestId = modelInfo.model_id;
                    else if (
                        fileName.StartsWith(
                            ModelFilePrefixes.DefaultChestOpen,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                        config.defaultOpenChestId = modelInfo.model_id;
                }
                finally
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
            }

            refreshPlaceableLibraryEvent.Raise();
            statusLabel.text = $"Downloaded {total} models successfully.";
            ToastNotification.Show(
                "Default models downloaded successfully!",
                "success",
                Color.green
            );
            CloseMenu();
        }
    }
}
