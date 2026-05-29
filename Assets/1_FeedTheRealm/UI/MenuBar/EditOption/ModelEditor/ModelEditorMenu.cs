using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.Repository;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FTR.Core.Common.Config;
using FTR.UI;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.MenuBar.EditOption.SettingsMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class ModelsEditorMenu : MenuController
    {
        [Inject]
        private ModelsRepository modelsRepository;

        [Inject]
        private RefreshPlaceableLibraryEvent refreshPlaceableLibraryEvent;

        [Inject]
        private Config config;

        [Inject]
        private PlaceablesLibrary placeablesLibrary;

        [Inject]
        private WorldPrefabProvider prefabProvider;

        [SerializeField]
        private VisualTreeAsset modelItemTemplate;

        private TextField modelsSearchField;
        private ScrollView modelsList;
        private Button closeButton;
        private Button modelsSearchClearBtn;
        private DropdownField defaultClosedChestDropdown;
        private DropdownField defaultOpenChestDropdown;
        private Button saveClosedModelButton;
        private Button saveOpenedModelButton;
        private List<StructureData> models;
        private List<PlaceableOption> options;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            modelsSearchField = root.Q<TextField>("ModelsSearch");
            modelsSearchField.RegisterValueChangedCallback(e => ApplyModelsSearch(e.newValue));
            closeButton = root.Q<Button>("Close");
            modelsList = root.Q<ScrollView>("ModelsList");
            defaultClosedChestDropdown = root.Q<DropdownField>("DefaultClosedChest");
            defaultOpenChestDropdown = root.Q<DropdownField>("DefaultOpenChest");
            saveClosedModelButton = root.Q<Button>("SaveClosedModel");
            saveOpenedModelButton = root.Q<Button>("SaveOpenedModel");

            closeButton.clicked += CloseMenu;
            saveClosedModelButton.clicked += OnSaveClosedModel;
            saveOpenedModelButton.clicked += OnSaveOpenedModel;

            modelsSearchClearBtn = root.Q<Button>("ModelSearchClear");

            modelsSearchField.RegisterValueChangedCallback(e =>
            {
                modelsSearchClearBtn.style.display = string.IsNullOrEmpty(e.newValue)
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
                ApplyModelsSearch(e.newValue);
            });

            modelsSearchClearBtn.clicked += () =>
            {
                modelsSearchField.SetValueWithoutNotify(string.Empty);
                modelsSearchClearBtn.style.display = DisplayStyle.None;
                RenderModelsList(models);
            };

            SetupChestModelDropdowns();
            PopulateModelsList();
        }

        void OnDisable()
        {
            closeButton.clicked -= CloseMenu;
            saveClosedModelButton.clicked -= OnSaveClosedModel;
            saveOpenedModelButton.clicked -= OnSaveOpenedModel;
        }

        private void SetupChestModelDropdowns()
        {
            options = placeablesLibrary.GetPlaceableOptions(PlaceableObjectCategories.Structure);
            var names = options.Select(o => o.displayName).ToList();

            defaultClosedChestDropdown.choices = names;
            defaultOpenChestDropdown.choices = names;

            if (!string.IsNullOrEmpty(config.defaultClosedChestId))
            {
                var match = options.FirstOrDefault(o => o.id == config.defaultClosedChestId);
                defaultClosedChestDropdown.SetValueWithoutNotify(match.displayName);
            }

            if (!string.IsNullOrEmpty(config.defaultOpenChestId))
            {
                var match = options.FirstOrDefault(o => o.id == config.defaultOpenChestId);
                defaultOpenChestDropdown.SetValueWithoutNotify(match.displayName);
            }
        }

        private void OnSaveClosedModel()
        {
            var selected = options.FirstOrDefault(o =>
                o.displayName == defaultClosedChestDropdown.value
            );
            config.defaultClosedChestId = selected.id;
            ToastNotification.Show("Default closed chest model saved.", "success", Color.green);
        }

        private void OnSaveOpenedModel()
        {
            var selected = options.FirstOrDefault(o =>
                o.displayName == defaultOpenChestDropdown.value
            );
            config.defaultOpenChestId = selected.id;
            ToastNotification.Show("Default open chest model saved.", "success", Color.green);
        }

        private void ApplyModelsSearch(string query)
        {
            var filtered = string.IsNullOrWhiteSpace(query)
                ? models
                : models
                    .Where(m =>
                        m.structureName.Contains(query, System.StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();

            RenderModelsList(filtered);
        }

        private void RenderModelsList(List<StructureData> source)
        {
            modelsList.Clear();

            foreach (var model in source)
            {
                var capturedModel = model;
                var entry = modelItemTemplate.Instantiate();

                entry.Q<Label>("ModelName").text = capturedModel.structureName;

                var hasColliders = entry.Q<Toggle>("HasColliders");
                var defaultScale = entry.Q<Vector3Field>("DefaultScale");
                var defaultRotation = entry.Q<Vector3Field>("DefaultRotation");
                var saveButton = entry.Q<Button>("Save");
                var deleteButton = entry.Q<Button>("DeleteModel");

                hasColliders.SetValueWithoutNotify(capturedModel.hasColliders);
                defaultScale.SetValueWithoutNotify(capturedModel.size);
                defaultRotation.SetValueWithoutNotify(capturedModel.rotation);

                saveButton.clicked += () =>
                {
                    capturedModel.hasColliders = hasColliders.value;
                    capturedModel.size = defaultScale.value;
                    capturedModel.rotation = defaultRotation.value;
                    modelsRepository.WriteModelToDisk(capturedModel);
                    ToastNotification.Show(
                        $"'{capturedModel.structureName}' saved.",
                        "success",
                        Color.green
                    );
                };

                deleteButton.clicked += () =>
                {
                    var confirmPopup = Instantiate(prefabProvider.confirmPopup);
                    var dialogController = confirmPopup.GetComponent<ConfirmPopupController>();
                    dialogController.Show(
                        title: "Delete Model",
                        question: $"Are you sure you want to delete '{capturedModel.structureName}'?",
                        onConfirm: () =>
                        {
                            modelsRepository.DeleteModel(capturedModel);
                            models.Remove(capturedModel);
                            entry.RemoveFromHierarchy();
                            refreshPlaceableLibraryEvent.Raise();
                            ToastNotification.Show(
                                $"'{capturedModel.structureName}' deleted.",
                                "success",
                                Color.green
                            );
                        },
                        onCancel: () => { }
                    );
                };

                modelsList.Add(entry);
            }
        }

        private void PopulateModelsList()
        {
            models = modelsRepository.GetModelsData().Values.ToList();
            modelsSearchField?.SetValueWithoutNotify(string.Empty);
            RenderModelsList(models);
        }
    }
}
