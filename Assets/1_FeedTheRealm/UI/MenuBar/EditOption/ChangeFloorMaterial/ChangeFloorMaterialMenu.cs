using System;
using System.Collections.Generic;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.Repository;
using FeedTheRealm.Core.WorldObjects;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.MenuBar.EditOption.SettingsMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class ChangeMaterialMenu : MenuController
    {
        [Inject]
        [SerializeField]
        private WorldSelector worldSelector;

        [Inject]
        [SerializeField]
        private DataPersistenceManager dataPersistenceManager;

        [SerializeField]
        private MaterialsRepository materialsRepository;

        [SerializeField]
        private WorldPrefabProvider worldPrefabProvider;

        [Header("Granularity")]
        [SerializeField]
        private float granularityMin = 1f;

        [SerializeField]
        private float granularityMax = 200f;

        [SerializeField]
        private float granularityDefault = 100f;

        private ScrollView materialsGrid;
        private TextField searchField;
        private Button closeButton;
        private Slider granularitySlider;
        private Label granularityValue;

        private List<Material> allMaterials = new();
        private VisualElement currentSelectedCard;
        private WorldControllerV2 worldController;

        private const string SelectedCardClass = "material-card--selected";

        void OnEnable()
        {
            worldController = FindFirstObjectByType<WorldControllerV2>();

            if (worldController == null)
                Debug.LogError("[ChangeMaterialMenu] WorldControllerV2 not found in scene.");

            var root = GetComponent<UIDocument>().rootVisualElement;

            materialsGrid = root.Q<ScrollView>("MaterialsGrid");
            searchField = root.Q<TextField>("SearchField");
            closeButton = root.Q<Button>("Close");
            granularitySlider = root.Q<Slider>("GranularitySlider");
            granularityValue = root.Q<Label>("GranularityValue");

            granularitySlider.lowValue = granularityMin;
            granularitySlider.highValue = granularityMax;
            granularitySlider.value = granularityDefault;
            granularityValue.text = Mathf.RoundToInt(granularityDefault).ToString();

            granularitySlider.RegisterValueChangedCallback(OnGranularityChanged);
            closeButton.clicked += CloseMenu;
            searchField.RegisterValueChangedCallback(OnSearchChanged);

            LoadMaterials();
        }

        void OnDisable()
        {
            closeButton.clicked -= CloseMenu;
            searchField.UnregisterValueChangedCallback(OnSearchChanged);
            granularitySlider.UnregisterValueChangedCallback(OnGranularityChanged);
        }

        // ── Data ──────────────────────────────────────────────────────────────

        private void LoadMaterials()
        {
            allMaterials = materialsRepository.GetAllMaterials();

            if (allMaterials.Count == 0)
                Debug.LogWarning("[ChangeMaterialMenu] MaterialsRepository is empty.");

            RenderGrid(allMaterials);
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            RenderGrid(materialsRepository.Search(evt.newValue));
        }

        private void OnGranularityChanged(ChangeEvent<float> evt)
        {
            granularityValue.text = Mathf.RoundToInt(evt.newValue).ToString();
            worldController?.ApplyTextureGranularity(evt.newValue);
        }

        // ── Grid ──────────────────────────────────────────────────────────────

        private void RenderGrid(List<Material> materials)
        {
            materialsGrid.Clear();
            foreach (var mat in materials)
                materialsGrid.Add(BuildMaterialCard(mat));
        }

        private VisualElement BuildMaterialCard(Material mat)
        {
            var card = new VisualElement();
            card.AddToClassList("material-card");

            // ── Swatch / texture preview ──
            var swatch = new VisualElement();
            swatch.AddToClassList("material-card__swatch");

            if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") is Texture2D tex)
            {
                swatch.style.backgroundImage = new StyleBackground(tex);
                swatch.style.backgroundSize = new StyleBackgroundSize(
                    new BackgroundSize(BackgroundSizeType.Cover)
                );
            }
            else
            {
                swatch.style.backgroundColor = new StyleColor(GetMaterialBaseColor(mat));
            }

            // ── Info block ──
            var info = new VisualElement();
            info.AddToClassList("material-card__info");

            var nameLabel = new Label(mat.name);
            nameLabel.AddToClassList("material-card__name");

            var badge = new Label(GetShaderShortName(mat.shader.name));
            badge.AddToClassList("material-card__badge");

            info.Add(nameLabel);
            info.Add(badge);

            card.Add(swatch);
            card.Add(info);

            // ✅ Apply immediately on click
            card.RegisterCallback<ClickEvent>(_ => SelectMaterial(mat.name, card));

            return card;
        }

        // ── Selection ─────────────────────────────────────────────────────────

        private void SelectMaterial(string mat, VisualElement card)
        {
            currentSelectedCard?.RemoveFromClassList(SelectedCardClass);
            currentSelectedCard = card;
            card.AddToClassList(SelectedCardClass);
            Debug.Log($"Selected material: {mat}");

            ApplyMaterial(mat);
        }

        private void ApplyMaterial(string mat)
        {
            if (worldController == null || mat == null)
                return;

            try
            {
                worldController.OnFloorMaterialChanged(mat, granularitySlider.value);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChangeMaterialMenu] Failed to apply material: {ex.Message}");
            }
        }

        // ── URP Helpers ───────────────────────────────────────────────────────

        private static Color GetMaterialBaseColor(Material mat)
        {
            if (mat.HasProperty("_BaseColor"))
                return mat.GetColor("_BaseColor");
            if (mat.HasProperty("_Color"))
                return mat.GetColor("_Color");
            return Color.gray;
        }

        private static string GetShaderShortName(string fullShaderName)
        {
            var parts = fullShaderName.Split('/');
            return parts[^1];
        }
    }
}
