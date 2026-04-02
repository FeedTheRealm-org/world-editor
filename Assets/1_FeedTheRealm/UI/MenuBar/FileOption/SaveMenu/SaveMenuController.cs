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

        private Button saveAllButton;
        private Button saveCreatablesButton;
        private Button saveZoneButton;
        private Button saveWorldButton;
        private Button closeButton;
        private TextField nameInput;
        private TextField descriptionInput;

        private WorldData currentWorldData;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            nameInput = root.Q<TextField>("NameInput");
            descriptionInput = root.Q<TextField>("WorldDescription");
            saveAllButton = root.Q<Button>("SaveAll");
            saveCreatablesButton = root.Q<Button>("SaveCreatables");
            saveZoneButton = root.Q<Button>("SaveZone");
            saveWorldButton = root.Q<Button>("SaveWorld");
            closeButton = root.Q<Button>("Close");

            closeButton.clicked += CloseMenu;
            saveAllButton.clicked += OnSaveAllClicked;
            saveCreatablesButton.clicked += OnSaveCreatablesClicked;
            saveZoneButton.clicked += OnSaveZoneClicked;
            saveWorldButton.clicked += OnSaveWorldClicked;

            string worldName = worldSelector.selectedWorld;
            currentWorldData = dataPersistenceManager.GetWorldData(worldName);

            if (currentWorldData != null)
            {
                nameInput.value = currentWorldData.worldName;
                descriptionInput.value = currentWorldData.description ?? "";
            }
            else if (!string.IsNullOrEmpty(worldName))
            {
                nameInput.value = worldName;
            }

            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool hasWorldData = currentWorldData != null;
            saveCreatablesButton.SetEnabled(hasWorldData);
            saveZoneButton.SetEnabled(hasWorldData);
            saveWorldButton.SetEnabled(hasWorldData);
        }

        void OnDisable()
        {
            closeButton.clicked -= CloseMenu;
            saveAllButton.clicked -= OnSaveAllClicked;
            saveCreatablesButton.clicked -= OnSaveCreatablesClicked;
            saveZoneButton.clicked -= OnSaveZoneClicked;
            saveWorldButton.clicked -= OnSaveWorldClicked;
        }

        private void OnSaveAllClicked()
        {
            try
            {
                string worldName = GetValidatedWorldName();
                EnsureWorldData(worldName);
                dataPersistenceManager.SaveWorldMetadata(currentWorldData);
                dataPersistenceManager.SaveZone(worldName, worldSelector.selectedZoneId);
                dataPersistenceManager.SaveCreatables(worldName);
                worldSelector.selectedWorld = worldName;
                ToastNotification.Show(
                    $"World {worldName} saved successfully",
                    "success",
                    Color.green
                );
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void OnSaveCreatablesClicked()
        {
            try
            {
                string worldName = GetValidatedWorldName();
                dataPersistenceManager.SaveCreatables(worldName);
                ToastNotification.Show("Creatables saved", "success", Color.green);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void OnSaveZoneClicked()
        {
            try
            {
                string worldName = GetValidatedWorldName();
                dataPersistenceManager.SaveZone(worldName, worldSelector.selectedZoneId);
                ToastNotification.Show(
                    $"Zone {worldSelector.selectedZoneId} saved",
                    "success",
                    Color.green
                );
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void OnSaveWorldClicked()
        {
            try
            {
                string worldName = GetValidatedWorldName();
                EnsureWorldData(worldName);
                dataPersistenceManager.SaveWorldMetadata(currentWorldData);
                worldSelector.selectedWorld = worldName;
                ToastNotification.Show("World data saved", "success", Color.green);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private string GetValidatedWorldName()
        {
            string worldName = nameInput?.value?.Trim();
            ValidateWorldName(worldName);
            return worldName;
        }

        private void EnsureWorldData(string worldName)
        {
            if (currentWorldData == null)
                currentWorldData = dataPersistenceManager.CreateNewWorld(worldName);

            currentWorldData.worldName = worldName;
            currentWorldData.description = descriptionInput?.value ?? "";
            currentWorldData.last_edited_at = DateTime.Now;
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

        private void ShowError(string message)
        {
            Debug.LogError($"Save error: {message}");
            ToastNotification.Show($"Error: {message}", "error", Color.red);
        }
    }
}
