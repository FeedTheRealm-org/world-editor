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
                if (child is Button b && b.name == data.zoneMaterialId)
                    b.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f));
            }
        }

        private void PopulateMaterialsGrid()
        {
            materialsGrid.Clear();

            var materials = zoneMaterialsRepository.GetMaterialNames();
            foreach (var materialName in materials)
            {
                var button = CreateMaterialButton(materialName);
                materialsGrid.Add(button);
            }
        }

        private Button CreateMaterialButton(string materialName)
        {
            var button = new Button();
            button.name = materialName;
            button.text = materialName;
            button.style.height = 40;
            button.style.marginBottom = 4;
            button.style.color = new StyleColor(Color.white);
            button.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
            button.style.borderTopLeftRadius = 5;
            button.style.borderTopRightRadius = 5;
            button.style.borderBottomLeftRadius = 5;
            button.style.borderBottomRightRadius = 5;

            button.clicked += () => OnMaterialSelected(materialName, button);
            return button;
        }

        private void OnMaterialSelected(string materialName, Button button)
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
            {
                if (child is Button b)
                    b.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
            }
            button.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f));

            ToastNotification.Show($"Material '{materialName}' applied.", "success", Color.green);
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
