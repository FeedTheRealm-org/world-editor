using System;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FTR.UI;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.EditorBar.ElementOption.DialogsMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class DialogCreatorMenu : MenuController
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [SerializeField]
        private GameObject dialogsMenuPrefab;

        private DialogData editingData;
        private EditBuffer<DialogData> editBuffer;
        private const int MaxDialogNameLength = 40;

        private TextField nameInput;
        private Button saveButton;
        private Button closeButton;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            nameInput = root.Q<TextField>("NameField");
            saveButton = root.Q<Button>("SaveButton");
            closeButton = root.Q<Button>("Close");

            root.Q<Button>("Return").clicked += ReturnToList;
            closeButton.clicked += CloseMenu;
            saveButton.clicked += CreateNewObject;
        }

        public void SetupEditor(Dialog dialog)
        {
            editingData = dialog.data;
            editBuffer = new EditBuffer<DialogData>(editingData);
            PopulateFields();
            BindEditMode();
            saveButton.clicked -= CreateNewObject;
            saveButton.clicked -= SaveExistingDialog;
            saveButton.text = "Save Dialog";
            saveButton.clicked += SaveExistingDialog;
        }

        private void PopulateFields()
        {
            if (editBuffer != null)
                nameInput.value = editBuffer.Working.name;
        }

        private void BindEditMode()
        {
            if (editBuffer != null)
                nameInput.RegisterValueChangedCallback(evt =>
                    editBuffer.Working.name = evt.newValue
                );
        }

        private bool ValidateDialogName(out string error)
        {
            if (string.IsNullOrEmpty(nameInput.value))
            {
                error = "Dialog name is required.";
                return false;
            }

            if (nameInput.value.Length > MaxDialogNameLength)
            {
                error = $"Dialog name must be at most {MaxDialogNameLength} characters.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private void CreateNewObject()
        {
            if (!ValidateDialogName(out var error))
            {
                ToastNotification.Show($"Failed to save dialog: {error}", "error", Color.red);
                return;
            }

            var dialogData = new DialogData(Guid.NewGuid().ToString(), nameInput.value);

            creatablesManager.Add(new Dialog(dialogData));
            ToastNotification.Show("Dialog created successfully!", "success", Color.green);
            ReturnToList();
        }

        private void SaveExistingDialog()
        {
            if (!ValidateDialogName(out var error))
            {
                ToastNotification.Show($"Failed to save dialog: {error}", "error", Color.red);
                return;
            }

            if (editBuffer != null)
                editBuffer.Commit();
            ToastNotification.Show("Dialog saved successfully!", "success", Color.green);
            ReturnToList();
        }

        private void ReturnToList()
        {
            OpenMenu(dialogsMenuPrefab);
        }
    }
}
