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
        private ScrollView groundMaterialsGrid;
        private ScrollView skyboxMaterialsGrid;
        private TabView tabView;
        private Button resetSkyboxButton;

        private string selectedGroundMaterialId;
        private string selectedSkyboxMaterialId;
        private ZoneTextureType activeTab = ZoneTextureType.Ground;
        private const float DEFAULT_GRANULARITY = 100f;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            resetSkyboxButton = root.Q<Button>("ResetSkybox");
            resetSkyboxButton.clicked += OnResetSkyboxClicked;

            closeButton = root.Q<Button>("Close");
            addTextureButton = root.Q<Button>("AddTexture");
            resetGranularityButton = root.Q<Button>("ResetGranulairy");
            granularitySlider = root.Q<Slider>("GranularitySlider");
            tabView = root.Q<TabView>();

            groundMaterialsGrid = root.Q<Tab>("Ground").Q<ScrollView>("MaterialsGrid");
            skyboxMaterialsGrid = root.Q<Tab>("Skybox").Q<ScrollView>("MaterialsGrid");

            closeButton.clicked += CloseMenu;
            addTextureButton.clicked += OnAddTextureClicked;
            resetGranularityButton.clicked += OnResetGranularityClicked;
            granularitySlider.RegisterValueChangedCallback(OnGranularityChanged);
            tabView.activeTabChanged += OnTabChanged;

            SyncWithActiveZone();
            PopulateGroundGrid();
            PopulateSkyboxGrid();
        }

        void OnDisable()
        {
            closeButton.clicked -= CloseMenu;
            addTextureButton.clicked -= OnAddTextureClicked;
            resetGranularityButton.clicked -= OnResetGranularityClicked;
            granularitySlider.UnregisterValueChangedCallback(OnGranularityChanged);
            tabView.activeTabChanged -= OnTabChanged;
        }

        private void OnTabChanged(Tab previous, Tab current)
        {
            activeTab = current.name == "Skybox" ? ZoneTextureType.Skybox : ZoneTextureType.Ground;
            resetGranularityButton.style.display =
                activeTab == ZoneTextureType.Ground ? DisplayStyle.Flex : DisplayStyle.None;
            granularitySlider.style.display =
                activeTab == ZoneTextureType.Ground ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SyncWithActiveZone()
        {
            var data = zoneManager.ZoneController.Data;

            // Sync ground
            if (!string.IsNullOrEmpty(data?.zoneMaterialId))
            {
                selectedGroundMaterialId = data.zoneMaterialId;
                granularitySlider.SetValueWithoutNotify(data.textureGranularity);
            }
            else
            {
                granularitySlider.SetValueWithoutNotify(DEFAULT_GRANULARITY);
            }

            // Sync skybox
            if (!string.IsNullOrEmpty(data?.skyboxMaterialId))
                selectedSkyboxMaterialId = data.skyboxMaterialId;
        }

        private void PopulateGroundGrid()
        {
            groundMaterialsGrid.Clear();
            foreach (var kvp in zoneMaterialsRepository.GetTextures(ZoneTextureType.Ground))
                groundMaterialsGrid.Add(
                    CreateMaterialContainer(kvp.Key, kvp.Value, ZoneTextureType.Ground)
                );
        }

        private void PopulateSkyboxGrid()
        {
            skyboxMaterialsGrid.Clear();
            foreach (var kvp in zoneMaterialsRepository.GetTextures(ZoneTextureType.Skybox))
                skyboxMaterialsGrid.Add(
                    CreateMaterialContainer(kvp.Key, kvp.Value, ZoneTextureType.Skybox)
                );
        }

        private void PopulateActiveGrid()
        {
            if (activeTab == ZoneTextureType.Ground)
                PopulateGroundGrid();
            else
                PopulateSkyboxGrid();
        }

        private VisualElement CreateMaterialContainer(
            string materialName,
            Texture2D texture,
            ZoneTextureType type
        )
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
            button.clicked += () => OnMaterialSelected(materialName, container, type);
            container.Add(button);

            bool isDefault =
                type == ZoneTextureType.Ground
                && materialName == zoneMaterialsRepository.DefaultMaterialId;
            if (!isDefault)
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
                deleteButton.clicked += () => OnDeleteMaterial(materialName, type);
                container.Add(deleteButton);
            }

            // Highlight if currently selected
            string selectedId =
                type == ZoneTextureType.Ground
                    ? selectedGroundMaterialId
                    : selectedSkyboxMaterialId;
            if (materialName == selectedId)
                container.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f));

            return container;
        }

        private void OnMaterialSelected(
            string materialName,
            VisualElement container,
            ZoneTextureType type
        )
        {
            var material = zoneMaterialsRepository.GetMaterial(materialName, type);
            if (material == null)
            {
                ToastNotification.Show($"Material '{materialName}' not found.", "error", Color.red);
                return;
            }

            var grid = type == ZoneTextureType.Ground ? groundMaterialsGrid : skyboxMaterialsGrid;
            foreach (var child in grid.Children())
                child.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
            container.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f));

            if (type == ZoneTextureType.Ground)
            {
                selectedGroundMaterialId = materialName;
                zoneManager.ZoneController.ChangeMaterial(material, materialName);
                zoneManager.ZoneController.ApplyTextureGranularity(granularitySlider.value);
            }
            else
            {
                selectedSkyboxMaterialId = materialName;
                zoneManager.ZoneController.SetSkyboxMaterial(material, materialName);
            }

            ToastNotification.Show($"'{materialName}' applied.", "success", Color.green);
        }

        private void OnDeleteMaterial(string materialName, ZoneTextureType type)
        {
            zoneMaterialsRepository.DeleteMaterial(materialName, type);

            if (type == ZoneTextureType.Ground && selectedGroundMaterialId == materialName)
            {
                var defaultMaterial = zoneMaterialsRepository.GetMaterial(
                    zoneMaterialsRepository.DefaultMaterialId,
                    ZoneTextureType.Ground
                );
                zoneManager.ZoneController.ChangeMaterial(
                    defaultMaterial,
                    zoneMaterialsRepository.DefaultMaterialId
                );
                selectedGroundMaterialId = zoneMaterialsRepository.DefaultMaterialId;
            }
            else if (type == ZoneTextureType.Skybox && selectedSkyboxMaterialId == materialName)
            {
                RenderSettings.skybox = null;
                selectedSkyboxMaterialId = null;
            }

            PopulateActiveGrid();
            ToastNotification.Show($"'{materialName}' deleted.", "success", Color.green);
        }

        private void OnResetSkyboxClicked()
        {
            RenderSettings.skybox = null;
            DynamicGI.UpdateEnvironment();
            zoneManager.ZoneController.Data.skyboxMaterialId = null;
            selectedSkyboxMaterialId = null;

            foreach (var child in skyboxMaterialsGrid.Children())
                child.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));

            ToastNotification.Show("Skybox reset to default.", "success", Color.green);
        }

        private void OnGranularityChanged(ChangeEvent<float> evt)
        {
            if (string.IsNullOrEmpty(selectedGroundMaterialId))
                return;
            zoneManager.ZoneController.ApplyTextureGranularity(evt.newValue);
        }

        private void OnResetGranularityClicked()
        {
            granularitySlider.value = DEFAULT_GRANULARITY;
            if (!string.IsNullOrEmpty(selectedGroundMaterialId))
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
                        zoneMaterialsRepository.AddMaterial(paths[0], activeTab);
                        PopulateActiveGrid();
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
