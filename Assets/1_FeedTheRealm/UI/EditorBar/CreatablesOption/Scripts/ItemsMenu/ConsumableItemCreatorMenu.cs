using System;
using System.IO;
using System.Linq;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FTR.Core.Common.Config;
using FTR.UI;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;
using VContainer;

namespace FeedTheRealm.UI.EditorBar.ElementOption.ItemsMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class ConsumableItemCreatorMenu : MenuController
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [Inject]
        private Config config;

        [SerializeField]
        private GameObject itemsMenuPrefab;

        private ConsumableItemData editingData;
        private EditBuffer<ConsumableItemData> editBuffer;

        private TextField nameInput;
        private TextField descriptionInput;
        private DropdownField effectTypeInput;
        private IntegerField valueInput;
        private FloatField durationInput;
        private FloatField cooldownInput;
        private IntegerField maxStackInput;
        private Image spritePreview;
        private string currentSpritePath;
        private Button saveButton;
        private Button closeButton;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            nameInput = root.Q<TextField>("NameField");
            descriptionInput = root.Q<TextField>("DescriptionField");
            effectTypeInput = root.Q<DropdownField>("EffectTypeField");
            effectTypeInput.choices = Enum.GetNames(typeof(EffectType)).ToList();
            effectTypeInput.value = EffectType.Heal.ToString();
            valueInput = root.Q<IntegerField>("EffectValueField");
            durationInput = root.Q<FloatField>("EffectDurationField");
            cooldownInput = root.Q<FloatField>("EffectCooldownField");
            maxStackInput = root.Q<IntegerField>("MaxStackField");
            spritePreview = root.Q<Image>("SpritePreview");
            saveButton = root.Q<Button>("SaveButton");
            closeButton = root.Q<Button>("Close");

            root.Q<Button>("LoadSprite").clicked += LoadSprite;
            root.Q<Button>("Return").clicked += ReturnToList;
            closeButton.clicked += CloseMenu;
            saveButton.clicked += CreateNewObject;
        }

        public void SetupEditor(ConsumableItem consumable)
        {
            editingData = consumable.data;
            editBuffer = new EditBuffer<ConsumableItemData>(editingData);
            currentSpritePath = editingData.spriteFilePath;

            PopulateFields();
            BindEditMode();

            saveButton.clicked -= CreateNewObject;
            saveButton.text = "Save";
            saveButton.clicked += SaveExistingObject;
        }

        private void PopulateFields()
        {
            var dataToDisplay = editBuffer != null ? editBuffer.Working : editingData;
            if (dataToDisplay == null)
                return;

            nameInput.value = dataToDisplay.name;
            descriptionInput.value = dataToDisplay.description;
            effectTypeInput.value = dataToDisplay.effectType.ToString();
            valueInput.value = dataToDisplay.value;
            durationInput.value = dataToDisplay.duration;
            cooldownInput.value = dataToDisplay.cooldown;
            maxStackInput.value = dataToDisplay.maxStack;
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
            effectTypeInput.RegisterValueChangedCallback(evt =>
                editBuffer.Working.effectType = Enum.Parse<EffectType>(evt.newValue)
            );
            valueInput.RegisterValueChangedCallback(evt => editBuffer.Working.value = evt.newValue);
            durationInput.RegisterValueChangedCallback(evt =>
                editBuffer.Working.duration = evt.newValue
            );
            cooldownInput.RegisterValueChangedCallback(evt =>
                editBuffer.Working.cooldown = evt.newValue
            );
            maxStackInput.RegisterValueChangedCallback(evt =>
                editBuffer.Working.maxStack = evt.newValue
            );
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
            if (durationInput.value < 0 || cooldownInput.value < 0 || maxStackInput.value < 0)
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

            var consumableData = new ConsumableItemData(
                itemData,
                Enum.Parse<EffectType>(effectTypeInput.value),
                valueInput.value,
                durationInput.value,
                cooldownInput.value,
                maxStackInput.value
            );

            creatablesManager.Add(new ConsumableItem(consumableData));
            ToastNotification.Show("Consumable item created successfully!", "success", Color.green);
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
                ToastNotification.Show(
                    "Consumable item saved successfully!",
                    "success",
                    Color.green
                );
                ReturnToList();
            }
        }

        private void ReturnToList() => OpenMenu(itemsMenuPrefab);
    }
}
