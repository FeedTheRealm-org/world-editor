using System;
using System.IO;
using System.Linq;
using Models;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;

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

        // Populate fields if editing existing item
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
        string spritePath = Path.Combine(
            Application.streamingAssetsPath,
            "Items",
            currentItem.spriteId + ".png"
        );
        Sprite sprite = LoadSpriteFromDisk(spritePath);
        if (FileBrowserHelpers.FileExists(spritePath) && sprite != null)
        {
            spritePreview.sprite = sprite;
        }
    }

    private void OnSaveClicked()
    {
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
                spriteId: ""
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
            currentItem.effectType = (EffectType)
                Enum.Parse(typeof(EffectType), effectTypeInput.value);
            logger.Log(
                $"Updated consumable item: {currentItem.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }
        SaveSprite();
        ReturnToItemsMenu();
    }

    private void ReturnToItemsMenu()
    {
        OpenMenu(itemsMenuPrefab);
    }

    private void LoadSprite()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("PNG Images", ".png"));
        FileBrowser.SetDefaultFilter(".png");
        FileBrowser.ShowLoadDialog(
            onSuccess: OnSpriteSelected,
            onCancel: () => logger.Log("Sprite selection canceled", this, Logging.LogType.Info),
            pickMode: FileBrowser.PickMode.Files,
            allowMultiSelection: false,
            initialPath: null,
            initialFilename: null,
            title: "Select Item Sprite",
            loadButtonText: "Select"
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

        Sprite sprite = LoadSpriteFromDisk(sourcePath);
        if (sprite == null)
        {
            logger.Log("Failed to load sprite for preview", this, Logging.LogType.Error);
            return;
        }

        spritePreview.sprite = sprite;
        pendingSpriteSourcePath = sourcePath;
        logger.Log("Sprite loaded for preview (not saved yet)", this, Logging.LogType.Info);
    }

    private void SaveSprite()
    {
        if (string.IsNullOrEmpty(pendingSpriteSourcePath))
            return;

        string targetDir = Path.Combine(Application.streamingAssetsPath, "Items");
        Directory.CreateDirectory(targetDir);
        string spriteId = currentItem.spriteId;
        string targetPath = Path.Combine(targetDir, spriteId + ".png");
        FileBrowserHelpers.CopyFile(pendingSpriteSourcePath, targetPath);
        pendingSpriteSourcePath = null;

        logger.Log($"Sprite saved to disk with id: {spriteId}", this, Logging.LogType.Info);
    }

    private Sprite LoadSpriteFromDisk(string path)
    {
        if (!FileBrowserHelpers.FileExists(path))
            return null;

        byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(path);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

        if (!texture.LoadImage(bytes))
            return null;

        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    void OnDisable()
    {
        if (saveButton != null)
            saveButton.clicked -= OnSaveClicked;
        if (returnButton != null)
            returnButton.clicked -= CloseMenu;
        if (closeButton != null)
            closeButton.clicked -= CloseMenu;
    }
}
