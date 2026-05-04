using System;
using System.Collections.Generic;
using FeedTheRealm.Core.Repository;
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
        [SerializeField]
        private MaterialsRepository materialsRepository;

        [SerializeField]
        private WorldPrefabProvider worldPrefabProvider;

        private ScrollView materialsGrid;
        private TextField searchField;
        private Button applyButton;
        private Button closeButton;
        private Label selectedMaterialLabel;
        private VisualElement footerSwatch;

        private List<Material> allMaterials = new();
        private Material pendingMaterial;
        private VisualElement currentSelectedCard;

        private const string SelectedCardClass = "material-card--selected";

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            materialsGrid = root.Q<ScrollView>("MaterialsGrid");
            searchField = root.Q<TextField>("SearchField");
            applyButton = root.Q<Button>("ApplyMaterial");
            closeButton = root.Q<Button>("Close");
            selectedMaterialLabel = root.Q<Label>("SelectedMaterialName");
            footerSwatch = root.Q<VisualElement>("SelectedSwatch");

            applyButton.SetEnabled(false);

            applyButton.clicked += OnApplyClicked;
            closeButton.clicked += CloseMenu;
            searchField.RegisterValueChangedCallback(OnSearchChanged);

            LoadMaterials();
        }

        void OnDisable()
        {
            applyButton.clicked -= OnApplyClicked;
            closeButton.clicked -= CloseMenu;
            searchField.UnregisterValueChangedCallback(OnSearchChanged);
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

            // ── Swatch ──
            var swatch = new VisualElement();
            swatch.AddToClassList("material-card__swatch");

            Color baseColor = GetMaterialBaseColor(mat);
            swatch.style.backgroundColor = new StyleColor(baseColor);

            // If a texture exists, tint slightly and show a hint label
            if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") != null)
            {
                swatch.style.backgroundColor = new StyleColor(baseColor * 0.85f);

                var texHint = new Label("tex");
                texHint.AddToClassList("material-card__tex-hint");
                swatch.Add(texHint);
            }

            // ── Labels ──
            var nameLabel = new Label(mat.name);
            nameLabel.AddToClassList("material-card__name");

            var badge = new Label(GetShaderShortName(mat.shader.name));
            badge.AddToClassList("material-card__badge");

            card.Add(swatch);
            card.Add(nameLabel);
            card.Add(badge);

            card.RegisterCallback<ClickEvent>(_ => SelectMaterial(mat, card));

            return card;
        }

        // ── Selection ─────────────────────────────────────────────────────────

        private void SelectMaterial(Material mat, VisualElement card)
        {
            currentSelectedCard?.RemoveFromClassList(SelectedCardClass);
            currentSelectedCard = card;
            card.AddToClassList(SelectedCardClass);

            pendingMaterial = mat;
            selectedMaterialLabel.text = mat.name;
            footerSwatch.style.backgroundColor = new StyleColor(GetMaterialBaseColor(mat));

            applyButton.SetEnabled(true);
        }

        private void OnApplyClicked()
        {
            if (pendingMaterial == null)
                return;

            try
            {
                var worldController = FindFirstObjectByType<WorldControllerV2>();

                if (worldController == null)
                {
                    Debug.LogError("[ChangeMaterialMenu] WorldControllerV2 not found in scene.");
                    return;
                }

                worldController.OnFloorMaterialChanged(pendingMaterial);
                ToastNotification.Show(
                    $"Material '{pendingMaterial.name}' applied!",
                    "success",
                    Color.green
                );
                CloseMenu();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChangeMaterialMenu] Failed to apply material: {ex.Message}");
                ToastNotification.Show(
                    $"Failed to apply material: {ex.Message}",
                    "error",
                    Color.red
                );
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
