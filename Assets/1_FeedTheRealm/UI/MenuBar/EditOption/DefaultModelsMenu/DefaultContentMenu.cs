using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using API;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.Repository;
using FeedTheRealm.UI.Common;
using FTR.Core.Common.Config;
using FTR.Core.Enums;
using FTR.Gameplay.DefaultContent;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.MenuBar.EditOption.SettingsMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class DefaultContentMenu : MenuController
    {
        [Inject]
        private ModelsRepository modelsRepository;

        [Inject]
        private ZoneMaterialsRepository zoneMaterialsRepository;

        [Inject]
        private ModelService modelService;

        [Inject]
        private MaterialService materialService;

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

            try
            {
                await DownloadDefaultModels();
                await DownloadDefaultMaterials();
                statusLabel.text = "All default content downloaded successfully.";
                ToastNotification.Show(
                    "Default content downloaded successfully!",
                    "success",
                    Color.green
                );
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[DefaultContentMenu] Failed to download default content: {ex.Message}"
                );
                ToastNotification.Show(
                    $"Failed to download default content: {ex.Message}",
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
            statusLabel.text = "Fetching default models...";
            var modelsInfo = await modelService.ListDefaultModels();

            int total = modelsInfo.Count;
            int current = 0;

            foreach (var kvp in modelsInfo)
            {
                var modelInfo = kvp.Value;
                string fileName = Path.GetFileName(modelInfo.url);
                string modelName = Path.GetFileNameWithoutExtension(modelInfo.url);

                current++;
                statusLabel.text = $"Downloading model {current}/{total}: {fileName}";

                string tempPath = await modelService.DownloadModel(modelInfo);
                if (string.IsNullOrEmpty(tempPath))
                {
                    Debug.LogWarning(
                        $"[DefaultContentMenu] {fileName} — download failed. Stopping model downloads."
                    );
                    return;
                }

                try
                {
                    var structureData = new StructureData(
                        id: modelInfo.model_id,
                        structureName: modelName,
                        size: Vector3.one,
                        rotation: Vector3.zero,
                        fileName: fileName,
                        hasColliders: true,
                        isDefault: true
                    );
                    DefaultContentHandler.ApplyPrefixConfig(fileName, structureData, config);

                    // Strip prefix after handler has read it
                    structureData.structureName = DefaultContentHandler.StripPrefix(modelName);
                    structureData.fileName = DefaultContentHandler.StripPrefix(fileName);
                    modelsRepository.AddModel(structureData, tempPath);
                }
                finally
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
            }

            refreshPlaceableLibraryEvent.Raise();
            statusLabel.text = $"Downloaded {total} models successfully.";
        }

        private async Task DownloadDefaultMaterials()
        {
            statusLabel.text = "Fetching default materials...";
            MaterialResponse[] materials = await materialService.GetMaterialsListAsync();

            if (materials == null || materials.Length == 0)
            {
                statusLabel.text = "No default materials found to download.";
                return;
            }

            int total = materials.Length;
            int current = 0;

            foreach (var material in materials)
            {
                current++;
                statusLabel.text = $"Downloading material {current}/{total}: {material.name}";

                string tempPath = await materialService.DownloadMaterialAsync(
                    material,
                    material.name
                );
                if (string.IsNullOrEmpty(tempPath))
                {
                    Debug.LogWarning(
                        $"[DefaultContentMenu] {material.name} — download failed. Stopping material downloads."
                    );
                    return;
                }
                try
                {
                    zoneMaterialsRepository.AddDefaultMaterial(tempPath);
                }
                finally
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
            }

            statusLabel.text = $"Downloaded {total} materials successfully.";
        }
    }
}
