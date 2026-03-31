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
    public class ConsumableItemCreatorMenu : MenuController
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [Inject]
        private Config config;

        [SerializeField]
        private GameObject itemsMenuPrefab;

        private ConsumableItemData editingData;

        private TextField nameInput;
        private TextField descriptionInput;
        private DropdownField effectTypeInput;
        private IntegerField valueInput;
        private FloatField durationInput;
        private FloatField cooldownInput;
        private IntegerField maxStackInput;
        private Image spritePreview;
        private string pendingSpritePath;
        private Button saveButton;
        private Button closeButton;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            nameInput = root.Q<TextField>("NameField");
            descriptionInput = root.Q<TextField>("DescriptionField");
            effectTypeInput = root.Q<DropdownField>("EffectTypeField");
            effectTypeInput.choices = Enum.GetNames(typeof(EffectType)).ToList();
            effectTypeInput.value = EffectType.None.ToString();
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
            effectTypeInput.value = editingData.effectType.ToString();
            valueInput.value = editingData.value;
            durationInput.value = editingData.duration;
            cooldownInput.value = editingData.cooldown;
            maxStackInput.value = editingData.maxStack;
            LoadExistingSprite(editingData.spriteFilePath);
        }

        private void BindEditMode()
        {
            nameInput.RegisterValueChangedCallback(evt => editingData.name = evt.newValue);
            descriptionInput.RegisterValueChangedCallback(evt =>
                editingData.description = evt.newValue
            );
            effectTypeInput.RegisterValueChangedCallback(evt =>
                editingData.effectType = Enum.Parse<EffectType>(evt.newValue)
            );
            valueInput.RegisterValueChangedCallback(evt => editingData.value = evt.newValue);
            durationInput.RegisterValueChangedCallback(evt => editingData.duration = evt.newValue);
            cooldownInput.RegisterValueChangedCallback(evt => editingData.cooldown = evt.newValue);
            maxStackInput.RegisterValueChangedCallback(evt => editingData.maxStack = evt.newValue);
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

        private void CreateNewObject()
        {
            var itemData = new ItemData(
                Guid.NewGuid().ToString(),
                nameInput.value,
                descriptionInput.value ?? "",
                pendingSpritePath
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
            ReturnToList();
        }

        private void ReturnToList() => OpenMenu(itemsMenuPrefab);
    }
}
