using System;
using System.IO;
using Models;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

[RequireComponent(typeof(UIDocument))]
public class NPCCreatorMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private GenericNPC currentNPC;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private GameObject npcMenuPrefab;

    private TextField nameInput;
    private TextField descriptionInput;
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
        saveButton = root.Q<Button>("SaveNPC");
        returnButton = root.Q<Button>("Return");
        closeButton = root.Q<Button>("Close");
        spritePreview = root.Q<Image>("SpritePreview");

        var npcPreviewContainer = root.Q<VisualElement>("NPCPreviewContainer");
        if (npcPreviewContainer != null)
        {
            loadSpriteButton = npcPreviewContainer.Q<Button>();
        }

        saveButton.clicked += OnSaveClicked;
        returnButton.clicked += ReturnToNPCsMenu;
        closeButton.clicked += CloseMenu;
        loadSpriteButton.clicked += LoadSprite;

        if (currentNPC == null)
        {
            currentNPC = EditContext.GetAndClearObjectToEdit<GenericNPC>();
        }

        // Populate fields if editing existing item
        if (currentNPC != null)
        {
            PopulateFields();
        }
    }

    private void PopulateFields()
    {
        nameInput.value = currentNPC.name;
        descriptionInput.value = currentNPC.description ?? "";

        // Load existing sprite for preview
        LoadExistingSprite(currentNPC.spriteFile);
    }

    private void LoadExistingSprite(string spritePath)
    {
        if (string.IsNullOrEmpty(spritePath))
            return;

        string absolutePath = spritePath;
        if (!Path.IsPathRooted(spritePath))
        {
            absolutePath = Path.Combine(Application.streamingAssetsPath, spritePath);
        }

        if (FileBrowserHelpers.FileExists(absolutePath))
        {
            Sprite sprite = FileHandler.LoadSpriteFromDisk(absolutePath);
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

    private void OnSaveClicked()
    {
        if (string.IsNullOrEmpty(nameInput.value))
        {
            logger?.Log("NPC name is required", this, Logging.LogType.Warning);
            ToastNotification.Show("NPC name is required", "error", Color.red);
            return;
        }

        if (currentNPC == null)
        {
            var npcData = new NPCData(
                null,
                nameInput.value,
                descriptionInput.value ?? "",
                pendingSpriteSourcePath
            );
            currentNPC = new GenericNPC(npcData);
            creatorObjectLibrary.AddCreatable(CreatorObjectCategories.NPC, currentNPC);
            logger.Log($"Created new NPC: {currentNPC.DisplayName}", this, Logging.LogType.Info);
        }
        else
        {
            currentNPC.name = nameInput.value;
            currentNPC.description = descriptionInput.value;
            if (!string.IsNullOrEmpty(pendingSpriteSourcePath))
            {
                currentNPC.spriteFile = pendingSpriteSourcePath;
            }
            logger.Log($"Updated NPC: {currentNPC.DisplayName}", this, Logging.LogType.Info);
        }

        ToastNotification.Show("NPC saved successfully", "success", Color.green);

        ReturnToNPCsMenu();
    }

    private void ReturnToNPCsMenu()
    {
        OpenMenu(npcMenuPrefab);
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
        logger.Log("Sprite loaded for preview (not saved yet)", this, Logging.LogType.Info);
    }

    void OnDisable()
    {
        if (saveButton != null)
            saveButton.clicked -= OnSaveClicked;
        if (returnButton != null)
            returnButton.clicked -= ReturnToNPCsMenu;
        if (closeButton != null)
            closeButton.clicked -= CloseMenu;
        if (loadSpriteButton != null)
            loadSpriteButton.clicked -= LoadSprite;
    }
}
