using System;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.MenuBar.FileOption.SaveMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class SaveMenuController : MenuController
    {
        [Inject]
        private WorldSelector worldSelector;

        [Inject]
        private DataPersistenceManager dataPersistenceManager;

        private Button saveButton;
        private Button closeButton;
        private TextField nameInput;
        private VisualElement root;

        private WorldData currentWorldData;

        void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;
            saveButton = root.Q<Button>("Save");
            closeButton = root.Q<Button>("Close");
            nameInput = root.Q<TextField>("NameInput");

            string worldName = worldSelector.selectedWorld;
            if (worldName != null && !string.IsNullOrEmpty(worldName))
            {
                nameInput.value = worldName;
            }

            saveButton.clicked += OnSaveClicked;
            closeButton.clicked += CloseMenu;

            // TODO: when saving, we can add the bio data here and other worlds metada to later update
            currentWorldData ??= dataPersistenceManager.GetWorldData(worldName);
        }

        private void OnDisable()
        {
            saveButton.clicked -= OnSaveClicked;
            closeButton.clicked -= CloseMenu;
        }

        private void OnSaveClicked()
        {
            try
            {
                string worldName = nameInput?.value?.Trim();
                ValidateWorldName(worldName);

                if (currentWorldData == null)
                    currentWorldData = dataPersistenceManager.CreateNewWorld(worldName);

                currentWorldData.last_edited_at = DateTime.Now;
                dataPersistenceManager.SaveWorldMetadata(currentWorldData);
                dataPersistenceManager.SaveZone(worldName, worldSelector.selectedZoneId);
                dataPersistenceManager.SaveCreatables(worldName);
                worldSelector.selectedWorld = worldName;
                ToastNotification.Show(
                    $"World {worldName} was saved successfully",
                    "success",
                    Color.green
                );
                Debug.Log($"World {currentWorldData} was saved successfully");
                CloseMenu();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving world: {ex.Message}");
                ToastNotification.Show($"Error saving world: {ex.Message}", "error", Color.red);
            }
        }

        private void ValidateWorldName(string worldName)
        {
            if (string.IsNullOrWhiteSpace(worldName))
                throw new ArgumentException("World name is required");

            if (!System.Text.RegularExpressions.Regex.IsMatch(worldName, @"^[a-zA-Z0-9\s\-_]+$"))
                throw new ArgumentException(
                    "World name can only contain letters, numbers, spaces, hyphens and underscores"
                );
        }
    }
}
