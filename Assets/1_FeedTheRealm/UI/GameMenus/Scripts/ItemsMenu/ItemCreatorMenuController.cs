using System;
using System.IO;
using System.Linq;
using Models;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

[RequireComponent(typeof(UIDocument))]
public class ItemCreatorMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private ConsumableItem currentItem;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private GameObject itemsMenuPrefab;

    private TextField nameInput;
    private TextField descriptionInput;
    private DropdownField effectTypeInput;
    private IntegerField valueInput;
    private FloatField durationInput;
    private FloatField cooldownInput;
    private IntegerField maxStackInput;
    private Button saveButton;
    private Button returnButton;
    private Button closeButton;
    private Button loadSpriteButton;
    private Image spritePreview;
    private string pendingSpriteSourcePath;

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // note: these if statements are helpful when debugging missing UI elements
        nameInput = root.Q<TextField>("NameField");
        if (nameInput == null)
            logger.Log("Name input field not found in UI", this, Logging.LogType.Error);
        descriptionInput = root.Q<TextField>("DescriptionField");
        if (descriptionInput == null)
            logger.Log("Description input field not found in UI", this, Logging.LogType.Error);
        effectTypeInput = root.Q<DropdownField>("EffectTypeField");
        if (effectTypeInput == null)
            logger.Log("Effect type dropdown field not found in UI", this, Logging.LogType.Error);
        valueInput = root.Q<IntegerField>("EffectValueField");
        if (valueInput == null)
            logger.Log("Effect value input field not found in UI", this, Logging.LogType.Error);
        durationInput = root.Q<FloatField>("EffectDurationField");
        if (durationInput == null)
            logger.Log("Effect duration input field not found in UI", this, Logging.LogType.Error);
        cooldownInput = root.Q<FloatField>("EffectCooldownField");
        if (cooldownInput == null)
            logger.Log("Effect cooldown input field not found in UI", this, Logging.LogType.Error);
        maxStackInput = root.Q<IntegerField>("MaxStackField");
        if (maxStackInput == null)
            logger.Log("Max stack input field not found in UI", this, Logging.LogType.Error);
        saveButton = root.Q<Button>("SaveItem");
        returnButton = root.Q<Button>("Return");
        closeButton = root.Q<Button>("Close");
        spritePreview = root.Q<Image>("SpritePreview");

        var itemPreviewContainer = root.Q<VisualElement>("ItemPreviewContainer");
        if (itemPreviewContainer != null)
        {
            loadSpriteButton = itemPreviewContainer.Q<Button>();
        }
        effectTypeInput.choices = Enum.GetNames(typeof(EffectType)).ToList();

        saveButton.clicked += OnSaveClicked;
        returnButton.clicked += ReturnToItemsMenu;
        closeButton.clicked += CloseMenu;
        loadSpriteButton.clicked += LoadSprite;

        if (currentItem != null)
        {
            PopulateFields();
        }
    }

    private void PopulateFields()
    {
        nameInput.value = currentItem.name;
        descriptionInput.value = currentItem.description ?? "";
        valueInput.value = currentItem.value;
        durationInput.value = currentItem.duration;
        cooldownInput.value = currentItem.cooldown;
        maxStackInput.value = currentItem.maxStack;
        effectTypeInput.value = currentItem.effectType.ToString();
        // Load existing sprite for preview
        string spritePath = currentItem.spriteFile;
        Sprite sprite = FileHandler.LoadSpriteFromDisk(spritePath);
        if (FileBrowserHelpers.FileExists(spritePath) && sprite != null)
        {
            spritePreview.sprite = sprite;
        }
    }

    private void OnSaveClicked()
    {
        string savedSpritePath = null;
        if (!string.IsNullOrEmpty(pendingSpriteSourcePath))
        {
            string itemId = currentItem != null ? currentItem.ObjectId : Guid.NewGuid().ToString();
            savedSpritePath = FileHandler.SaveFile(pendingSpriteSourcePath, "Sprites", itemId);
        }

        if (currentItem == null)
        {
            var itemData = new ConsumableItemData(
                null,
                name: nameInput.value,
                description: descriptionInput.value ?? "",
                effectType: (EffectType)Enum.Parse(typeof(EffectType), effectTypeInput.value),
                value: valueInput.value,
                duration: durationInput.value,
                cooldown: cooldownInput.value,
                maxStack: maxStackInput.value,
                spriteFilepath: savedSpritePath
            );
            currentItem = new ConsumableItem(itemData);
            creatorObjectLibrary.AddCreatable(CreatorObjectCategories.ConsumableItem, currentItem);
            logger.Log(
                $"Created new consumable item: {currentItem.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }
        else
        {
            currentItem.name = nameInput.value;
            currentItem.description = descriptionInput.value;
            currentItem.value = valueInput.value;
            currentItem.duration = durationInput.value;
            currentItem.cooldown = cooldownInput.value;
            currentItem.maxStack = maxStackInput.value;
            currentItem.spriteFile = savedSpritePath ?? currentItem.spriteFile;
            currentItem.effectType = (EffectType)
                Enum.Parse(typeof(EffectType), effectTypeInput.value);
            logger.Log(
                $"Updated consumable item: {currentItem.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }
        ReturnToItemsMenu();
    }

    private void ReturnToItemsMenu()
    {
        OpenMenu(itemsMenuPrefab);
    }

    private void LoadSprite()
    {
        FileHandler.ShowFilePickerDialog(
            onSuccess: OnSpriteSelected,
            onCancel: () => logger.Log("Sprite selection canceled", this, Logging.LogType.Info)
        );
    }

    private void OnSpriteSelected(string[] paths)
    {
        if (paths == null || paths.Length == 0)
            return;
        string sourcePath = paths[0];

        if (!sourcePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            logger.Log("Selected file is not a PNG", this, Logging.LogType.Warning);
            return;
        }
        Sprite sprite = FileHandler.LoadSpriteFromDisk(sourcePath);
        if (sprite == null)
        {
            logger.Log("Failed to load sprite for preview", this, Logging.LogType.Error);
            return;
        }
        spritePreview.sprite = sprite;
        pendingSpriteSourcePath = sourcePath;
        logger.Log(
            "Sprite loaded for preview (not saved yet) | filepath" + sourcePath,
            this,
            Logging.LogType.Info
        );
    }

    void OnDisable()
    {
        if (saveButton != null)
            saveButton.clicked -= OnSaveClicked;
        if (returnButton != null)
            returnButton.clicked -= ReturnToItemsMenu;
        if (closeButton != null)
            closeButton.clicked -= CloseMenu;
        if (loadSpriteButton != null)
            loadSpriteButton.clicked -= LoadSprite;
    }
}
