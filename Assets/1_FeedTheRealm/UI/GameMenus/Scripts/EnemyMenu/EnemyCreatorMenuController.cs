using System;
using System.IO;
using System.Linq;
using Models;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class EnemyCreatorMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private GenericEnemy currentEnemy;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private GameObject enemyMenuPrefab;

    private TextField nameInput;
    private TextField descriptionInput;
    private IntegerField healthPointsInput;
    private IntegerField damageInput;
    private IntegerField speedInput;
    private IntegerField rangeInput;
    private DropdownField lootTableInput;
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
        healthPointsInput = root.Q<IntegerField>("HealthPoints");
        if (healthPointsInput == null)
            logger.Log("Health points input field not found in UI", this, Logging.LogType.Error);
        damageInput = root.Q<IntegerField>("AttackDamage");
        if (damageInput == null)
            logger.Log("Attack damage input field not found in UI", this, Logging.LogType.Error);
        speedInput = root.Q<IntegerField>("Speed");
        if (speedInput == null)
            logger.Log("Speed input field not found in UI", this, Logging.LogType.Error);
        rangeInput = root.Q<IntegerField>("Range");
        if (rangeInput == null)
            logger.Log("Range input field not found in UI", this, Logging.LogType.Error);
        lootTableInput = root.Q<DropdownField>("LootTableField");
        if (lootTableInput == null)
            logger.Log("LootTable dropdown field not found in UI", this, Logging.LogType.Error);
        else
        {
            // Populate loot table choices from library
            var lootTables = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.LootTable)
                .Cast<LootTable>()
                .ToList();
            lootTableInput.choices = lootTables.Select(lt => lt.DisplayName).ToList();
        }
        saveButton = root.Q<Button>("SaveEnemy");
        returnButton = root.Q<Button>("Return");
        closeButton = root.Q<Button>("Close");
        spritePreview = root.Q<Image>("SpritePreview");

        var enemyPreviewContainer = root.Q<VisualElement>("EnemyPreviewContainer");
        if (enemyPreviewContainer != null)
        {
            loadSpriteButton = enemyPreviewContainer.Q<Button>();
        }
        saveButton.clicked += OnSaveClicked;
        returnButton.clicked += ReturnToItemsMenu;
        closeButton.clicked += CloseMenu;
        loadSpriteButton.clicked += LoadSprite;

        // Populate fields if editing existing item
        if (currentEnemy != null)
        {
            PopulateFields();
        }
    }

    private void PopulateFields()
    {
        nameInput.value = currentEnemy.name;
        descriptionInput.value = currentEnemy.description ?? "";
        healthPointsInput.value = currentEnemy.healthPoints;
        damageInput.value = currentEnemy.damage;
        speedInput.value = currentEnemy.speed;
        rangeInput.value = currentEnemy.range;
        if (currentEnemy.lootTable != null)
            lootTableInput.value = currentEnemy.lootTable.name;
        // Load existing sprite for preview
        string spritePath = Path.Combine(
            Application.streamingAssetsPath,
            "Items",
            currentEnemy.spriteId + ".png"
        );
        Sprite sprite = LoadSpriteFromDisk(spritePath);
        if (FileBrowserHelpers.FileExists(spritePath) && sprite != null)
        {
            spritePreview.sprite = sprite;
        }
    }

    private void OnSaveClicked()
    {
        // Find selected loot table by name
        var lootTables = creatorObjectLibrary
            .GetCreatables(CreatorObjectCategories.LootTable)
            .Cast<LootTable>()
            .ToList();
        var selectedLootTable = lootTables.FirstOrDefault(lt =>
            lt.DisplayName == lootTableInput.value
        );

        // Create LootTableData from selected LootTable (if any)
        LootTableData lootTableData = null;
        if (selectedLootTable != null)
        {
            lootTableData = new LootTableData(
                selectedLootTable.ObjectId,
                selectedLootTable.DisplayName,
                selectedLootTable.minGoldDropAmount,
                selectedLootTable.maxGoldDropAmount,
                selectedLootTable.lootItems
            );
        }
        if (currentEnemy == null)
        {
            var enemyData = new EnemyData(
                null,
                nameInput.value,
                descriptionInput.value ?? "",
                healthPointsInput.value,
                damageInput.value,
                speedInput.value,
                rangeInput.value,
                "", // spriteId, set after sprite is saved
                lootTableData
            );
            currentEnemy = new GenericEnemy(enemyData);
            creatorObjectLibrary.AddCreatable(CreatorObjectCategories.Enemy, currentEnemy);
            logger.Log(
                $"Created new enemy: {currentEnemy.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }
        else
        {
            currentEnemy.name = nameInput.value;
            currentEnemy.description = descriptionInput.value;
            currentEnemy.healthPoints = healthPointsInput.value;
            currentEnemy.damage = damageInput.value;
            currentEnemy.speed = speedInput.value;
            currentEnemy.range = rangeInput.value;
            currentEnemy.lootTable = lootTableData;
            logger.Log($"Updated enemy: {currentEnemy.DisplayName}", this, Logging.LogType.Info);
        }
        SaveSprite();
        ReturnToItemsMenu();
    }

    private void ReturnToItemsMenu()
    {
        OpenMenu(enemyMenuPrefab);
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
        // Ensure spriteId is initialized; use the enemy's persistent ObjectId by default.
        string spriteId = currentEnemy.spriteId;
        if (string.IsNullOrEmpty(spriteId))
        {
            spriteId = currentEnemy.ObjectId;
            currentEnemy.spriteId = spriteId;
        }
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
