using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.Repository;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FeedTheRealm.UI.Common;
using FTR.Core.Common.Config;
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

        [SerializeField]
        private VisualTreeAsset modelItemTemplate;

        private ListView modelsList;
        private Button closeButton;
        private DropdownField defaultClosedChestDropdown;
        private DropdownField defaultOpenChestDropdown;
        private Button saveClosedModelButton;
        private Button saveOpenedModelButton;
        private List<StructureData> models;
        private List<PlaceableOption> options;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            closeButton = root.Q<Button>("Close");
            modelsList = root.Q<ListView>("ModelsList");
            defaultClosedChestDropdown = root.Q<DropdownField>("DefaultClosedChest");
            defaultOpenChestDropdown = root.Q<DropdownField>("DefaultOpenChest");
            saveClosedModelButton = root.Q<Button>("SaveClosedModel");
            saveOpenedModelButton = root.Q<Button>("SaveOpenedModel");

            closeButton.clicked += CloseMenu;
            saveClosedModelButton.clicked += OnSaveClosedModel;
            saveOpenedModelButton.clicked += OnSaveOpenedModel;

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

        private void PopulateModelsList()
        {
            models = modelsRepository.GetModelsData().Values.ToList();

            modelsList.makeItem = () =>
            {
                var entry = modelItemTemplate.Instantiate();
                entry.userData = new System.Action[2];
                return entry;
            };

            modelsList.bindItem = (entry, index) =>
            {
                var model = models[index];
                var callbacks = (System.Action[])entry.userData;

                entry.Q<Label>("ModelName").text = model.structureName;

                var hasColliders = entry.Q<Toggle>("HasColliders");
                hasColliders.SetValueWithoutNotify(model.hasColliders);

                var defaultScale = entry.Q<Vector3Field>("DefaultScale");
                defaultScale.SetValueWithoutNotify(model.size);

                var defaultRotation = entry.Q<Vector3Field>("DefaultRotation");
                defaultRotation.SetValueWithoutNotify(model.rotation);

                var saveButton = entry.Q<Button>("Save");
                var deleteButton = entry.Q<Button>("DeleteModel");

                if (callbacks[0] != null)
                    saveButton.clicked -= callbacks[0];
                if (callbacks[1] != null)
                    deleteButton.clicked -= callbacks[1];

                callbacks[0] = () =>
                {
                    model.hasColliders = hasColliders.value;
                    model.size = defaultScale.value;
                    model.rotation = defaultRotation.value;
                    modelsRepository.WriteModelToDisk(model);
                    ToastNotification.Show(
                        $"'{model.structureName}' saved.",
                        "success",
                        Color.green
                    );
                };

                callbacks[1] = () =>
                {
                    modelsRepository.DeleteModel(model);
                    models.Remove(model);
                    modelsList.RefreshItems();
                    refreshPlaceableLibraryEvent.Raise();
                    ToastNotification.Show(
                        $"'{model.structureName}' deleted.",
                        "success",
                        Color.green
                    );
                };

                saveButton.clicked += callbacks[0];
                deleteButton.clicked += callbacks[1];
            };

            modelsList.itemsSource = models;
            modelsList.selectionType = SelectionType.None;
            modelsList.RefreshItems();
        }
    }
}
