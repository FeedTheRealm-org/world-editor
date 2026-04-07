using System.Collections.Generic;
using System.IO;
using System.Linq;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.Repository;
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

        [SerializeField]
        private VisualTreeAsset modelItemTemplate;

        private ListView modelsList;
        private Button closeButton;
        private List<StructureData> models;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            closeButton = root.Q<Button>("Close");
            modelsList = root.Q<ListView>("ModelsList");

            closeButton.clicked += CloseMenu;
            PopulateModelsList();
        }

        void OnDisable()
        {
            closeButton.clicked -= CloseMenu;
        }

        private void PopulateModelsList()
        {
            models = modelsRepository.GetModelsData().Values.ToList();

            modelsList.makeItem = () =>
            {
                var entry = modelItemTemplate.Instantiate();
                // store callbacks as user data so we can replace them
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

                // unregister previous callbacks before adding new ones
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
