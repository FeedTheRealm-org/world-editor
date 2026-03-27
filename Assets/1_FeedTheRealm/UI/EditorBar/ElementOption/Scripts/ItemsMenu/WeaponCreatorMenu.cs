using System;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;
using VContainer;

namespace FeedTheRealm.UI.EditorBar.ElementOption.ItemsMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class WeaponCreatorMenu : MenuController
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [SerializeField]
        private GameObject itemsMenuPrefab;

        private WeaponItemData editingData;

        private TextField nameInput;
        private TextField descriptionInput;
        private EnumField weaponTypeInput;
        private IntegerField damageInput;
        private FloatField attackSpeedInput;
        private FloatField rangeInput;
        private IntegerField ammoInput;
        private Image spritePreview;
        private string pendingSpritePath;
        private Button saveButton;
        private Button closeButton;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            nameInput = root.Q<TextField>("NameField");
            descriptionInput = root.Q<TextField>("DescriptionField");
            weaponTypeInput = root.Q<EnumField>("WeaponType");
            damageInput = root.Q<IntegerField>("Damage");
            attackSpeedInput = root.Q<FloatField>("AttackSpeed");
            rangeInput = root.Q<FloatField>("Range");
            ammoInput = root.Q<IntegerField>("Ammo");
            spritePreview = root.Q<Image>("SpritePreview");
            saveButton = root.Q<Button>("SaveButton");
            closeButton = root.Q<Button>("Close");

            root.Q<Button>("LoadSprite").clicked += LoadSprite;
            closeButton.clicked += ReturnToList;
            saveButton.clicked += CreateNewObject;
        }

        public void SetupEditor(Weapon weapon)
        {
            editingData = weapon.data;
            PopulateFields();
            BindEditMode();
            saveButton.clicked -= CreateNewObject;
            saveButton.text = "Return to List";
            saveButton.clicked += ReturnToList;
        }

        private void PopulateFields()
        {
            nameInput.value = editingData.name;
            descriptionInput.value = editingData.description;
            weaponTypeInput.value = editingData.weaponType;
            damageInput.value = editingData.damage;
            attackSpeedInput.value = editingData.attackSpeed;
            rangeInput.value = editingData.range;
            ammoInput.value = editingData.ammo;
            LoadExistingSprite(editingData.spriteFilePath);
        }

        private void BindEditMode()
        {
            nameInput.RegisterValueChangedCallback(evt => editingData.name = evt.newValue);
            descriptionInput.RegisterValueChangedCallback(evt =>
                editingData.description = evt.newValue
            );
            weaponTypeInput.RegisterValueChangedCallback(evt =>
                editingData.weaponType = (WeaponType)evt.newValue
            );
            damageInput.RegisterValueChangedCallback(evt => editingData.damage = evt.newValue);
            attackSpeedInput.RegisterValueChangedCallback(evt =>
                editingData.attackSpeed = evt.newValue
            );
            rangeInput.RegisterValueChangedCallback(evt => editingData.range = evt.newValue);
            ammoInput.RegisterValueChangedCallback(evt => editingData.ammo = evt.newValue);
        }

        private void LoadSprite()
        {
            CustomFileBrowser.ShowFilePickerDialog(
                onSuccess: paths =>
                {
                    if (paths == null || paths.Length == 0)
                        return;
                    if (!paths[0].EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogWarning("Selected file is not a PNG.");
                        return;
                    }
                    var sprite = CustomFileBrowser.LoadSpriteFromDisk(paths[0]);
                    if (sprite == null)
                        return;
                    spritePreview.sprite = sprite;
                    pendingSpritePath = paths[0];
                    if (editingData != null)
                        editingData.spriteFilePath = pendingSpritePath;
                },
                onCancel: () => Debug.Log("Sprite selection canceled.")
            );
        }

        private void LoadExistingSprite(string spritePath)
        {
            if (string.IsNullOrEmpty(spritePath))
                return;
            var sprite = CustomFileBrowser.LoadSpriteFromDisk(spritePath);
            if (sprite != null)
                spritePreview.sprite = sprite;
        }

        private void CreateNewObject()
        {
            var itemData = new ItemData(
                Guid.NewGuid().ToString(),
                nameInput.value,
                descriptionInput.value ?? "",
                pendingSpritePath
            );

            var weaponData = new WeaponItemData(
                itemData,
                (WeaponType)weaponTypeInput.value,
                damageInput.value,
                attackSpeedInput.value,
                rangeInput.value,
                ammoInput.value
            );

            creatablesManager.Add(new Weapon(weaponData));
            ReturnToList();
        }

        private void ReturnToList() => OpenMenu(itemsMenuPrefab);
    }
}
