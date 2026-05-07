using System;
using System.IO;
using FeedTheRealm.Core.Repository;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;
using VContainer;

namespace FeedTheRealm.UI.MenuBar.EditOption.ChangeFloorMaterial
{
    [RequireComponent(typeof(UIDocument))]
    public class ZoneEditor : MenuController
    {
        [Inject]
        private ZoneManager zoneManager;

        [Inject]
        private ZoneMaterialsRepository zoneMaterialsRepository;

        private Button closeButton;
        private Button addTextureButton;
        private Button resetGranularityButton;
        private Slider granularitySlider;
        private ScrollView materialsGrid;

        private string selectedMaterialId;
        private const float DEFAULT_GRANULARITY = 100f;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            closeButton = root.Q<Button>("Close");
            addTextureButton = root.Q<Button>("AddTexture");
            resetGranularityButton = root.Q<Button>("ResetGranulairy");
            granularitySlider = root.Q<Slider>("GranularitySlider");
            materialsGrid = root.Q<ScrollView>("MaterialsGrid");

            closeButton.clicked += CloseMenu;
            addTextureButton.clicked += OnAddTextureClicked;
            resetGranularityButton.clicked += OnResetGranularityClicked;
            granularitySlider.RegisterValueChangedCallback(OnGranularityChanged);

            SyncWithActiveZone();
            PopulateMaterialsGrid();
        }

        void OnDisable()
        {
            closeButton.clicked -= CloseMenu;
            addTextureButton.clicked -= OnAddTextureClicked;
            resetGranularityButton.clicked -= OnResetGranularityClicked;
            granularitySlider.UnregisterValueChangedCallback(OnGranularityChanged);
        }

        private void SyncWithActiveZone()
        {
            var data = zoneManager.ZoneController.Data;
            if (data == null || string.IsNullOrEmpty(data.zoneMaterialId))
            {
                granularitySlider.SetValueWithoutNotify(DEFAULT_GRANULARITY);
                return;
            }

            selectedMaterialId = data.zoneMaterialId;
            granularitySlider.SetValueWithoutNotify(data.textureGranularity);

            foreach (var child in materialsGrid.Children())
            {
                var btn = child.Q<Button>();
                if (btn != null && btn.name == data.zoneMaterialId)
                    child.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f));
            }
        }

        private void PopulateMaterialsGrid()
        {
            materialsGrid.Clear();
            foreach (var kvp in zoneMaterialsRepository.GetTextures())
                materialsGrid.Add(CreateMaterialContainer(kvp.Key, kvp.Value));
        }

        private VisualElement CreateMaterialContainer(string materialName, Texture2D texture)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.justifyContent = Justify.SpaceBetween;
            container.style.alignItems = Align.Center;
            container.style.height = 40;
            container.style.marginBottom = 4;
            container.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
            container.style.borderTopLeftRadius = 5;
            container.style.borderTopRightRadius = 5;
            container.style.borderBottomLeftRadius = 5;
            container.style.borderBottomRightRadius = 5;

            // Texture preview
            if (texture != null)
            {
                var preview = new VisualElement();
                preview.style.width = 36;
                preview.style.height = 36;
                preview.style.marginLeft = 2;
                preview.style.borderTopLeftRadius = 4;
                preview.style.borderTopRightRadius = 4;
                preview.style.borderBottomLeftRadius = 4;
                preview.style.borderBottomRightRadius = 4;
                preview.style.backgroundImage = new StyleBackground(texture);
                container.Add(preview);
            }

            // Material name button
            var button = new Button();
            button.name = materialName;
            button.text = materialName;
            button.style.flexGrow = 1;
            button.style.color = new StyleColor(Color.white);
            button.style.backgroundColor = new StyleColor(Color.clear);
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            button.style.borderRightWidth = 0;
            button.clicked += () => OnMaterialSelected(materialName, container);
            container.Add(button);

            // Delete button (not shown for default)
            if (materialName != zoneMaterialsRepository.DefaultMaterialId)
            {
                var deleteButton = new Button();
                deleteButton.text = "✕";
                deleteButton.style.color = new StyleColor(Color.red);
                deleteButton.style.backgroundColor = new StyleColor(Color.clear);
                deleteButton.style.borderTopWidth = 0;
                deleteButton.style.borderBottomWidth = 0;
                deleteButton.style.borderLeftWidth = 0;
                deleteButton.style.borderRightWidth = 0;
                deleteButton.style.width = 30;
                deleteButton.clicked += () => OnDeleteMaterial(materialName);
                container.Add(deleteButton);
            }

            return container;
        }

        private void OnMaterialSelected(string materialName, VisualElement container)
        {
            var material = zoneMaterialsRepository.GetMaterial(materialName);
            if (material == null)
            {
                ToastNotification.Show($"Material '{materialName}' not found.", "error", Color.red);
                return;
            }

            selectedMaterialId = materialName;
            zoneManager.ZoneController.ChangeMaterial(material, materialName);
            zoneManager.ZoneController.ApplyTextureGranularity(granularitySlider.value);

            foreach (var child in materialsGrid.Children())
                child.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));

            container.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f));
            ToastNotification.Show($"Material '{materialName}' applied.", "success", Color.green);
        }

        private void OnDeleteMaterial(string materialName)
        {
            zoneMaterialsRepository.DeleteMaterial(materialName);

            if (selectedMaterialId == materialName)
            {
                var defaultMaterial = zoneMaterialsRepository.GetMaterial(
                    zoneMaterialsRepository.DefaultMaterialId
                );
                zoneManager.ZoneController.ChangeMaterial(
                    defaultMaterial,
                    zoneMaterialsRepository.DefaultMaterialId
                );
                selectedMaterialId = zoneMaterialsRepository.DefaultMaterialId;
            }

            PopulateMaterialsGrid();
            ToastNotification.Show($"'{materialName}' deleted.", "success", Color.green);
        }

        private void OnGranularityChanged(ChangeEvent<float> evt)
        {
            if (string.IsNullOrEmpty(selectedMaterialId))
                return;
            zoneManager.ZoneController.ApplyTextureGranularity(evt.newValue);
        }

        private void OnResetGranularityClicked()
        {
            granularitySlider.value = DEFAULT_GRANULARITY;
            if (!string.IsNullOrEmpty(selectedMaterialId))
                zoneManager.ZoneController.ApplyTextureGranularity(DEFAULT_GRANULARITY);
        }

        private void OnAddTextureClicked()
        {
            CustomFileBrowser.ShowFilePickerDialog(
                onSuccess: paths =>
                {
                    if (paths == null || paths.Length == 0)
                        return;

                    try
                    {
                        zoneMaterialsRepository.AddMaterial(paths[0]);
                        PopulateMaterialsGrid();
                        ToastNotification.Show(
                            $"Texture '{Path.GetFileNameWithoutExtension(paths[0])}' added successfully!",
                            "success",
                            Color.green
                        );
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ZoneEditor] Failed to add texture: {ex.Message}");
                        ToastNotification.Show(
                            $"Failed to add texture: {ex.Message}",
                            "error",
                            Color.red
                        );
                    }
                },
                onCancel: () => { },
                extensions: new[] { ".png", ".jpg", ".jpeg" }
            );
        }
    }
}
