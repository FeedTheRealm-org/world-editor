using System;
using System.IO;
using System.Linq;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using FTR.Core.Common.Config;
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

        [Inject]
        private Config config;

        private WeaponItemData editingData;
        private EditBuffer<WeaponItemData> editBuffer;

        private TextField nameInput;
        private TextField descriptionInput;
        private DropdownField weaponTypeInput;
        private DropdownField subWeaponTypeInput;
        private IntegerField damageInput;
        private FloatField attackSpeedInput;
        private FloatField rangeInput;
        private IntegerField ammoInput;
        private FloatField reloadSpeedInput;
        private Image spritePreview;
        private string currentSpritePath;
        private Button saveButton;
        private Button closeButton;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            nameInput = root.Q<TextField>("NameField");
            descriptionInput = root.Q<TextField>("DescriptionField");
            weaponTypeInput = root.Q<DropdownField>("WeaponTypeField");
            subWeaponTypeInput = root.Q<DropdownField>("SubWeaponTypeField");
            damageInput = root.Q<IntegerField>("DamageField");
            attackSpeedInput = root.Q<FloatField>("AttackSpeedField");
            rangeInput = root.Q<FloatField>("RangeField");
            ammoInput = root.Q<IntegerField>("AmmoField");
            reloadSpeedInput = root.Q<FloatField>("ReloadSpeedField");
            spritePreview = root.Q<Image>("SpritePreview");
            saveButton = root.Q<Button>("SaveButton");
            closeButton = root.Q<Button>("Close");

            root.Q<Button>("LoadSprite").clicked += LoadSprite;
            root.Q<Button>("Return").clicked += ReturnToList;
            closeButton.clicked += CloseMenu;
            saveButton.clicked += CreateNewObject;

            SetupDefaults();
        }

        private void SetupDefaults()
        {
            weaponTypeInput.choices = Enum.GetNames(typeof(WeaponType)).ToList();
            weaponTypeInput.value = WeaponType.Melee.ToString();

            subWeaponTypeInput.choices = Enum.GetNames(typeof(ValidSubMeleeWeaponType)).ToList();
            subWeaponTypeInput.value = SubWeaponType.HandHeld.ToString();

            HandleChangeWeaponType(WeaponType.Melee);
            weaponTypeInput.RegisterValueChangedCallback(evt =>
                HandleChangeWeaponType(Enum.Parse<WeaponType>(evt.newValue))
            );
            subWeaponTypeInput.RegisterValueChangedCallback(evt =>
            {
                if (editBuffer != null)
                    editBuffer.Working.subWeaponType = Enum.Parse<SubWeaponType>(evt.newValue);
            });
        }

        public void SetupEditor(Weapon weapon)
        {
            editingData = weapon.data;
            editBuffer = new EditBuffer<WeaponItemData>(editingData);
            currentSpritePath = editingData.spriteFilePath;

            BindEditMode();
            PopulateFields();

            HandleChangeWeaponType(editingData.weaponType);

            saveButton.clicked -= CreateNewObject;
            saveButton.text = "Save Weapon";
            saveButton.clicked += SaveExistingObject;
        }

        private void PopulateFields()
        {
            var dataToDisplay = editBuffer != null ? editBuffer.Working : editingData;
            if (dataToDisplay == null)
                return;

            nameInput.value = dataToDisplay.name;
            descriptionInput.value = dataToDisplay.description;
            weaponTypeInput.value = dataToDisplay.weaponType.ToString();
            subWeaponTypeInput.value = dataToDisplay.subWeaponType.ToString();
            damageInput.value = dataToDisplay.damage;
            attackSpeedInput.value = dataToDisplay.attackSpeed;
            rangeInput.value = dataToDisplay.range;
            ammoInput.value = dataToDisplay.ammo;
            reloadSpeedInput.value = dataToDisplay.reloadSpeed;
            LoadExistingSprite(dataToDisplay.spriteFilePath);
        }

        private void BindEditMode()
        {
            if (editBuffer == null)
                return;

            nameInput.RegisterValueChangedCallback(evt => editBuffer.Working.name = evt.newValue);
            descriptionInput.RegisterValueChangedCallback(evt =>
                editBuffer.Working.description = evt.newValue
            );
            damageInput.RegisterValueChangedCallback(evt =>
                editBuffer.Working.damage = evt.newValue
            );
            attackSpeedInput.RegisterValueChangedCallback(evt =>
                editBuffer.Working.attackSpeed = evt.newValue
            );
            rangeInput.RegisterValueChangedCallback(evt => editBuffer.Working.range = evt.newValue);
            ammoInput.RegisterValueChangedCallback(evt => editBuffer.Working.ammo = evt.newValue);
            reloadSpeedInput.RegisterValueChangedCallback(evt =>
                editBuffer.Working.reloadSpeed = evt.newValue
            );
        }

        private void HandleChangeWeaponType(WeaponType newType)
        {
            if (editBuffer != null)
                editBuffer.Working.weaponType = newType;
            switch (newType)
            {
                case WeaponType.Melee:
                    subWeaponTypeInput.choices = Enum.GetNames(typeof(ValidSubMeleeWeaponType))
                        .ToList();
                    subWeaponTypeInput.value = SubWeaponType.HandHeld.ToString();
                    if (editBuffer != null)
                        editBuffer.Working.subWeaponType = SubWeaponType.HandHeld;

                    ammoInput.visible = false;
                    reloadSpeedInput.visible = false;
                    break;
                case WeaponType.Ranged:
                    Debug.Log("Switched to Ranged: showing ammo and reload speed fields");
                    subWeaponTypeInput.choices = Enum.GetNames(typeof(ValidSubRangedWeaponType))
                        .ToList();
                    subWeaponTypeInput.value = SubWeaponType.HandHeld.ToString();
                    if (editBuffer != null)
                        editBuffer.Working.subWeaponType = SubWeaponType.HandHeld;

                    ammoInput.visible = true;
                    reloadSpeedInput.visible = true;
                    ammoInput.value = editBuffer != null ? editBuffer.Working.ammo : 0;
                    reloadSpeedInput.value =
                        editBuffer != null ? editBuffer.Working.reloadSpeed : 0f;
                    Debug.Log(
                        $"Ranged weapon selected: ammo and reload speed fields are now visible. Ammo value: {ammoInput.value}, Reload Speed value: {reloadSpeedInput.value}"
                    );
                    break;
                default:
                    throw new Exception($"Unhandled weapon type: {newType}");
            }
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
                    currentSpritePath = paths[0];
                    if (editingData != null)
                        editingData.spriteFilePath = currentSpritePath;
                },
                onCancel: () => Debug.Log("Sprite selection canceled.")
            );
        }

        private void LoadExistingSprite(string spritePath)
        {
            string fullPath = Path.Combine(config.SpritesDirectory, spritePath);
            if (string.IsNullOrEmpty(spritePath))
            {
                Debug.Log($"No existing sprite path found at: {fullPath}", this);
                return;
            }
            var sprite = CustomFileBrowser.LoadSpriteFromDisk(fullPath);
            if (sprite != null)
                spritePreview.sprite = sprite;
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrEmpty(currentSpritePath))
            {
                ToastNotification.Show("Failed to save: sprite is required.", "error", Color.red);
                return false;
            }
            if (string.IsNullOrEmpty(nameInput.value))
            {
                ToastNotification.Show("Failed to save: name is required.", "error", Color.red);
                return false;
            }
            if (
                damageInput.value < 0
                || attackSpeedInput.value < 0
                || rangeInput.value < 0
                || ammoInput.value < 0
                || reloadSpeedInput.value < 0
            )
            {
                ToastNotification.Show(
                    "Failed to save: some stats cannot be negative.",
                    "error",
                    Color.red
                );
                return false;
            }
            return true;
        }

        private void CreateNewObject()
        {
            if (!ValidateFields())
                return;

            var itemData = new ItemData(
                Guid.NewGuid().ToString(),
                nameInput.value,
                descriptionInput.value ?? "",
                currentSpritePath
            );

            var weaponData = new WeaponItemData(
                itemData,
                Enum.Parse<WeaponType>(weaponTypeInput.value),
                Enum.Parse<SubWeaponType>(subWeaponTypeInput.value),
                damageInput.value,
                attackSpeedInput.value,
                rangeInput.value,
                ammoInput.value,
                reloadSpeedInput.value
            );

            creatablesManager.Add(new Weapon(weaponData));
            ToastNotification.Show("Weapon item created successfully!", "success", Color.green);
            ReturnToList();
        }

        private void SaveExistingObject()
        {
            if (!ValidateFields())
                return;

            if (editBuffer != null)
            {
                editBuffer.Working.spriteFilePath = currentSpritePath;
                editBuffer.Commit();
                ToastNotification.Show("Weapon item saved successfully!", "success", Color.green);
                ReturnToList();
            }
        }

        private void ReturnToList() => OpenMenu(itemsMenuPrefab);
    }
}
