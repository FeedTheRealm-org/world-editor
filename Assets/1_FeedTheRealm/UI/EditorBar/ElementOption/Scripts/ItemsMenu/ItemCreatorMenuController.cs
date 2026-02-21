using System;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

[RequireComponent(typeof(UIDocument))]
public abstract class ItemCreatorMenuController<TItem> : MenuController
    where TItem : Item
{
    [SerializeField]
    protected Logging.Logger logger;

    [SerializeField]
    protected TItem currentItem;

    [SerializeField]
    protected CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    protected GameObject itemsMenuPrefab;

    protected TextField nameInput;
    protected TextField descriptionInput;
    protected Button saveButton;
    protected Button returnButton;
    protected Button closeButton;
    protected Button loadSpriteButton;
    protected Image spritePreview;
    protected string pendingSpriteSourcePath;

    protected abstract CreatorObjectCategories Category { get; }

    protected virtual void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // Common UI elements
        nameInput = root.Q<TextField>("NameField");
        if (nameInput == null)
            logger.Log("Name input field not found in UI", this, Logging.LogType.Error);
        descriptionInput = root.Q<TextField>("DescriptionField");
        if (descriptionInput == null)
            logger.Log("Description input field not found in UI", this, Logging.LogType.Error);

        saveButton = root.Q<Button>("SaveItem");
        returnButton = root.Q<Button>("Return");
        closeButton = root.Q<Button>("Close");
        spritePreview = root.Q<Image>("SpritePreview");

        var itemPreviewContainer = root.Q<VisualElement>("ItemPreviewContainer");
        if (itemPreviewContainer != null)
        {
            loadSpriteButton = itemPreviewContainer.Q<Button>();
        }

        InitializeSpecificFields(root);

        if (saveButton != null)
            saveButton.clicked += OnSaveClicked;
        if (returnButton != null)
            returnButton.clicked += ReturnToItemsMenu;
        if (closeButton != null)
            closeButton.clicked += CloseMenu;
        if (loadSpriteButton != null)
            loadSpriteButton.clicked += LoadSprite;

        if (currentItem == null)
        {
            currentItem = EditContext.GetAndClearObjectToEdit<TItem>();
        }

        if (currentItem != null)
        {
            PopulateFields();
        }
    }

    protected abstract void InitializeSpecificFields(VisualElement root);

    protected abstract void PopulateFields();

    protected abstract void OnSaveClicked();

    protected string SaveSpriteIfNeeded()
    {
        if (!string.IsNullOrEmpty(pendingSpriteSourcePath))
        {
            string itemId = currentItem != null ? currentItem.ObjectId : Guid.NewGuid().ToString();
            return FileHandler.SaveFile(pendingSpriteSourcePath, "Sprites", itemId);
        }
        return null;
    }

    protected void LoadExistingSprite(string spritePath)
    {
        if (string.IsNullOrEmpty(spritePath))
            return;

        string absolutePath = spritePath;
        if (!System.IO.Path.IsPathRooted(spritePath))
        {
            absolutePath = System.IO.Path.Combine(
                UnityEngine.Application.streamingAssetsPath,
                spritePath
            );
        }

        if (FileBrowserHelpers.FileExists(absolutePath))
        {
            Sprite sprite = CustomFileBrowser.LoadSpriteFromDisk(absolutePath);
            if (sprite != null)
            {
                spritePreview.sprite = sprite;
            }
            else
            {
                logger?.Log(
                    $"Failed to load sprite from: {absolutePath}",
                    this,
                    Logging.LogType.Warning
                );
            }
        }
        else
        {
            logger?.Log($"Sprite file not found at: {absolutePath}", this, Logging.LogType.Warning);
        }
    }

    protected void ReturnToItemsMenu()
    {
        OpenMenu(itemsMenuPrefab);
    }

    protected virtual void LoadSprite()
    {
        CustomFileBrowser.ShowFilePickerDialog(
            onSuccess: OnSpriteSelected,
            onCancel: () => logger.Log("Sprite selection canceled", this, Logging.LogType.Info)
        );
    }

    protected virtual void OnSpriteSelected(string[] paths)
    {
        if (paths == null || paths.Length == 0)
            return;
        string sourcePath = paths[0];

        if (!sourcePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            logger.Log("Selected file is not a PNG", this, Logging.LogType.Warning);
            return;
        }
        Sprite sprite = CustomFileBrowser.LoadSpriteFromDisk(sourcePath);
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

    protected virtual void OnDisable()
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
