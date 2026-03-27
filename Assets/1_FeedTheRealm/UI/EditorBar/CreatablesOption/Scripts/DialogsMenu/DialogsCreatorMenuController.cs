using System;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
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
            PopulateFields();
            BindEditMode();
            saveButton.clicked -= CreateNewObject;
            saveButton.text = "Return to List";
            saveButton.clicked += ReturnToList;
        }

        private void PopulateFields()
        {
            nameInput.value = editingData.name;
        }

        private void BindEditMode()
        {
            nameInput.RegisterValueChangedCallback(evt => editingData.name = evt.newValue);
        }

        private void CreateNewObject()
        {
            var dialogData = new DialogData(Guid.NewGuid().ToString(), nameInput.value);

            creatablesManager.Add(new Dialog(dialogData));
            ReturnToList();
        }

        private void ReturnToList() => OpenMenu(dialogsMenuPrefab);
    }
}
