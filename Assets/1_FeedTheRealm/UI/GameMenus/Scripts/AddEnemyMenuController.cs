using System;
using System.Collections.Generic;
using System.IO;
using FeedTheRealm.Utils;
using Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(UIDocument))]
public class AddEnemyMenuController : MenuController
{
    //[SerializeField] private Maker player;
    [SerializeField]
    private EnemyLibrarySO enemyDatabase;

    [SerializeField]
    private ConsumableItemLibrarySO consumableItemsDatabase;

    [SerializeField]
    private Logging.Logger logger;

    private Button addButton;
    private Button closeButton;
    private Button loadSpriteButton;
    private TextField nameInput;
    private TextField descriptionInput;
    private FloatField healthPointsField;
    private FloatField damageField;
    private IntegerField speedField;
    private DropdownField canMoveDropdown;
    private IntegerField rangeField;
    private TextField spritePathInput;
    private Image spritePreview;
    private VisualElement root;
    private Label nameErrorLabel;
    private VisualElement enemySettingsContent;
    private VisualElement enemyLootContent;
    private Button settingsTab;
    private Button lootTab;

    // Loot UI
    private DropdownField itemListDropdown;
    private FloatField itemMaxAmountField;
    private SliderInt dropChanceSlider;
    private FloatField goldAmountField;
    private Button addItemToEnemyButton;
    private Foldout itemsAddedFoldout;
    private Image lootSpritePreview;

    // Temporary loot data while editing
    private List<EnemyLootItem> currentLootItems = new List<EnemyLootItem>();

    // When >= 0, we are editing an existing enemy in the database at this index
    private int editingIndex = -1;

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        if (root == null)
        {
            logger.Log(
                "AddItemMenuController: UIDocument has no visual tree. Assign a UXML to the Source Asset.",
                this,
                Logging.LogType.Error
            );
            return;
        }

        addButton = root.Q<Button>("Add");
        closeButton = root.Q<Button>("Close");
        loadSpriteButton = root.Q<Button>("LoadSprite");

        nameInput = root.Q<TextField>("NameInput");
        descriptionInput = root.Q<TextField>("Description");
        healthPointsField = root.Q<FloatField>("HealthPoints");
        damageField = root.Q<FloatField>("Damage");
        speedField = root.Q<IntegerField>("Speed");
        canMoveDropdown = root.Q<DropdownField>("CanMove");
        rangeField = root.Q<IntegerField>("Range");
        spritePathInput = root.Q<TextField>("SpritePath");

        // Re-query tab contents every time, to avoid stale references
        enemySettingsContent = root.Q<VisualElement>("EnemySettingsContent");
        enemyLootContent = root.Q<VisualElement>("EnemyLootContent");

        // Sprite preview for base settings
        spritePreview =
            enemySettingsContent != null
                ? enemySettingsContent.Q<Image>("SpritePreview")
                : root.Q<Image>("SpritePreview");

        // Loot UI references (inside EnemyLootContent)
        if (enemyLootContent != null)
        {
            itemListDropdown = enemyLootContent.Q<DropdownField>("ItemList");
            itemMaxAmountField = enemyLootContent.Q<FloatField>("MaxDropAmount");
            dropChanceSlider = enemyLootContent.Q<SliderInt>("DropChance");
            goldAmountField = enemyLootContent.Q<FloatField>("GoldAmount");
            addItemToEnemyButton = enemyLootContent.Q<Button>("AddItem");
            itemsAddedFoldout = enemyLootContent.Q<Foldout>("ItemsAdded");
            lootSpritePreview = enemyLootContent.Q<Image>("SpritePreview");
        }

        settingsTab = root.Q<Button>("SettingsTab");
        lootTab = root.Q<Button>("LootTab");

        if (settingsTab != null)
            settingsTab.clicked += ShowSettingsTab;
        if (lootTab != null)
            lootTab.clicked += ShowLootTab;

        ShowSettingsTab();

        if (canMoveDropdown != null)
        {
            canMoveDropdown.choices = new List<string> { "true", "false" };
            if (string.IsNullOrEmpty(canMoveDropdown.value))
                canMoveDropdown.value = "true";
        }

        SetupLootUI();

        if (addButton != null)
            addButton.clicked += OnAddClicked;
        if (closeButton != null)
            closeButton.clicked += OnCloseClicked;
        if (loadSpriteButton != null)
            loadSpriteButton.clicked += OnLoadSpriteClicked;

        // Default to add mode when menu is opened
        editingIndex = -1;
        currentLootItems.Clear();
        if (addButton != null)
            addButton.text = "Add Enemy";
    }

    private void OnDisable()
    {
        if (addButton != null)
            addButton.clicked -= OnAddClicked;
        if (closeButton != null)
            closeButton.clicked -= OnCloseClicked;
        if (loadSpriteButton != null)
            loadSpriteButton.clicked -= OnLoadSpriteClicked;
        if (settingsTab != null)
            settingsTab.clicked -= ShowSettingsTab;
        if (lootTab != null)
            lootTab.clicked -= ShowLootTab;
        if (addItemToEnemyButton != null)
            addItemToEnemyButton.clicked -= OnAddLootItemClicked;
    }

    private void SetupLootUI()
    {
        // Slider: show numeric value
        if (dropChanceSlider != null)
        {
            dropChanceSlider.showInputField = true;
        }

        // Populate item dropdown from ConsumableItems database
        if (itemListDropdown != null && consumableItemsDatabase != null)
        {
            var allItems = consumableItemsDatabase.GetAllConsumableItems();
            var names = new List<string>();
            foreach (var item in allItems)
            {
                if (item != null && !string.IsNullOrEmpty(item.name))
                    names.Add(item.name);
            }
            itemListDropdown.choices = names;
            if (names.Count > 0 && string.IsNullOrEmpty(itemListDropdown.value))
            {
                itemListDropdown.value = names[0];
            }

            itemListDropdown.RegisterValueChangedCallback(evt =>
                UpdateLootPreviewSprite(evt.newValue)
            );
            if (!string.IsNullOrEmpty(itemListDropdown.value))
            {
                UpdateLootPreviewSprite(itemListDropdown.value);
            }
        }

        if (addItemToEnemyButton != null)
        {
            addItemToEnemyButton.clicked += OnAddLootItemClicked;
        }

        if (itemsAddedFoldout != null)
        {
            itemsAddedFoldout.text = "Items added";
        }
    }

    /// <summary>
    /// Initializes the form for editing an existing enemy at the given index.
    /// </summary>
    public void BeginEditEnemy(int index)
    {
        if (enemyDatabase == null)
        {
            logger.Log(
                "AddEnemyMenuController.BeginEditEnemy: enemyDatabase is not assigned.",
                this,
                Logging.LogType.Error
            );
            return;
        }

        var list = enemyDatabase.GetAllEnemies();
        if (list == null || index < 0 || index >= list.Count)
        {
            logger.Log(
                $"AddEnemyMenuController.BeginEditEnemy: index {index} is out of range.",
                this,
                Logging.LogType.Warning
            );
            return;
        }

        editingIndex = index;
        var enemy = list[index];
        if (enemy == null)
        {
            logger.Log(
                $"AddEnemyMenuController.BeginEditEnemy: enemy at index {index} is null.",
                this,
                Logging.LogType.Warning
            );
            return;
        }

        // Fill basic fields
        if (nameInput != null)
            nameInput.value = enemy.name;
        if (descriptionInput != null)
            descriptionInput.value = enemy.description;
        if (healthPointsField != null)
            healthPointsField.value = enemy.healthPoints;
        if (damageField != null)
            damageField.value = enemy.damage;
        if (speedField != null)
            speedField.value = enemy.speed;
        if (canMoveDropdown != null)
            canMoveDropdown.value = enemy.canMove ? "true" : "false";
        if (rangeField != null)
            rangeField.value = enemy.range;

        // Sprite id + preview
        string spriteId = enemy.spriteId ?? string.Empty;
        if (spritePathInput != null)
            spritePathInput.value = spriteId;

        if (!string.IsNullOrEmpty(spriteId))
        {
            string resolved = SpriteStorage.GetFilePathFromIdOrPath(spriteId);
            Sprite sprite = null;
            if (
                !string.IsNullOrEmpty(resolved)
                && (Path.IsPathRooted(resolved) || File.Exists(resolved))
            )
            {
                sprite = LoadSpriteFromAbsoluteFile(resolved);
            }
            else
            {
                sprite = Resources.Load<Sprite>(spriteId);
            }

            if (sprite != null && spritePreview != null)
            {
                spritePreview.sprite = sprite;
                spritePreview.image = sprite.texture;
            }
        }

        // Loot + gold
        currentLootItems =
            enemy.lootItems != null
                ? new List<EnemyLootItem>(enemy.lootItems)
                : new List<EnemyLootItem>();

        if (goldAmountField != null)
            goldAmountField.value = enemy.goldAmount;

        RefreshItemsAddedFoldoutUI();

        if (addButton != null)
            addButton.text = "Update Enemy";
        ShowSettingsTab();
    }

    private void OnAddClicked()
    {
        if (enemyDatabase == null)
        {
            logger.Log(
                "AddEnemyMenuController: enemyDatabase is not assigned. Assign it in the Inspector.",
                this,
                Logging.LogType.Error
            );
            return;
        }

        if (!ValidateName())
            return;

        var enemyData = BuildEnemyFromUI();
        if (enemyData == null)
            return;

        var list = enemyDatabase.GetAllEnemies();

        // If editingIndex is set, update the existing enemy instead of adding a new one
        if (editingIndex >= 0 && list != null && editingIndex < list.Count)
        {
            list[editingIndex] = enemyData;

#if UNITY_EDITOR
            if (enemyDatabase != null)
            {
                EditorUtility.SetDirty(enemyDatabase);
                AssetDatabase.SaveAssets();
            }
#endif

            logger.Log(
                $"AddEnemyMenuController: Updated enemy '{enemyData.name}' at index {editingIndex}.",
                this
            );
        }
        else
        {
            logger.Log(
                $"AddEnemyMenuController: Adding enemy '{enemyData.name}' (HP {enemyData.healthPoints}, Damage {enemyData.damage})",
                this
            );

            try
            {
                enemyDatabase.AddEnemy(enemyData);
            }
            catch (Exception ex)
            {
                logger.Log(
                    $"AddEnemyMenuController: Failed to add enemy to database: {ex.Message}",
                    this,
                    Logging.LogType.Error
                );
                return;
            }
        }

        editingIndex = -1;
        currentLootItems.Clear();
        CloseMenu();
    }

    private void ShowSettingsTab()
    {
        if (enemySettingsContent != null)
            enemySettingsContent.style.display = DisplayStyle.Flex;
        if (enemyLootContent != null)
            enemyLootContent.style.display = DisplayStyle.None;
    }

    private void ShowLootTab()
    {
        if (enemySettingsContent != null)
            enemySettingsContent.style.display = DisplayStyle.None;
        if (enemyLootContent != null)
            enemyLootContent.style.display = DisplayStyle.Flex;
    }

    private bool ValidateName()
    {
        string itemName = nameInput != null ? nameInput.value?.Trim() : string.Empty;
        if (string.IsNullOrEmpty(itemName))
        {
            ShowNameRequiredMessage();
            return false;
        }
        ClearNameError();
        return true;
    }

    private void ShowNameRequiredMessage()
    {
        logger.Log("Enemy name cannot be empty.", this, Logging.LogType.Error);

        if (nameInput != null)
            nameInput.Focus();

        if (root == null)
            return;

        if (nameErrorLabel == null)
        {
            nameErrorLabel = new Label("MessageErrorName") { name = "NameError" };
            nameErrorLabel.style.color = new StyleColor(Color.red);
            nameErrorLabel.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
        }
        else
        {
            nameErrorLabel.text = "Name is required";
        }

        var parent = nameInput != null ? nameInput.parent : root;
        try
        {
            int idx = parent.IndexOf(nameInput);
            if (idx >= 0)
                parent.Insert(idx + 1, nameErrorLabel);
            else
                parent.Add(nameErrorLabel);
        }
        catch
        {
            parent.Add(nameErrorLabel);
        }
    }

    private void ClearNameError()
    {
        if (nameErrorLabel != null && nameErrorLabel.parent != null)
            nameErrorLabel.RemoveFromHierarchy();
    }

    private void OnLoadSpriteClicked()
    {
        StartCoroutine(
            SpriteFilePicker.WaitForSpriteFilePanel(
                "Select Sprite",
                selected =>
                {
                    if (string.IsNullOrEmpty(selected) || spritePathInput == null)
                        return;

                    Debug.Log($"AddEnemyMenuController: Selected sprite file '{selected}'");

                    // Save file into persistent storage and get generated UUID
                    string id = SpriteStorage.SaveFileReturnId(selected);
                    if (!string.IsNullOrEmpty(id))
                        spritePathInput.value = id;
                    else
                        spritePathInput.value = selected;

                    // Preview from resolved id or from the source file
                    string resolved = SpriteStorage.GetFilePathFromIdOrPath(
                        !string.IsNullOrEmpty(id) ? id : selected
                    );
                    var previewSprite = LoadSpriteFromAbsoluteFile(
                        !string.IsNullOrEmpty(resolved) ? resolved : selected
                    );
                    if (previewSprite != null)
                    {
                        spritePreview.sprite = previewSprite;
                        spritePreview.image = previewSprite.texture;
                    }
                }
            )
        );
    }

    private EnemyData BuildEnemyFromUI()
    {
        string itemName = nameInput != null ? nameInput.value?.Trim() : string.Empty;
        if (string.IsNullOrEmpty(itemName))
        {
            logger.Log("Enemy name cannot be empty.", this, Logging.LogType.Error);
            return null;
        }

        string desc = descriptionInput != null ? descriptionInput.value?.Trim() : string.Empty;
        int hp = healthPointsField != null ? Mathf.RoundToInt(healthPointsField.value) : 0;
        int dmg = damageField != null ? Mathf.RoundToInt(damageField.value) : 0;
        int spd = speedField != null ? speedField.value : 0;
        bool canMove = canMoveDropdown != null && canMoveDropdown.value == "true";
        int rng = rangeField != null ? rangeField.value : 0;

        string spriteId = spritePathInput != null ? spritePathInput.value?.Trim() : string.Empty;
        Sprite sprite = null;
        if (!string.IsNullOrEmpty(spriteId))
        {
            // Resolve id (or treat input as path) to an absolute file path
            string resolved = SpriteStorage.GetFilePathFromIdOrPath(spriteId);
            if (
                !string.IsNullOrEmpty(resolved)
                && (Path.IsPathRooted(resolved) || File.Exists(resolved))
            )
            {
                sprite = LoadSpriteFromAbsoluteFile(resolved);
            }
            else
            {
                // If resolution failed, try Resources.Load using the provided value
                sprite = Resources.Load<Sprite>(spriteId);
            }

            if (sprite != null && spritePreview != null)
            {
                spritePreview.sprite = sprite;
                spritePreview.image = sprite.texture;
            }
        }

        if (sprite != null)
            Debug.Log($"Try add Enemy with Sprite='{sprite.name}'");
        else
            Debug.LogWarning($"Try add Enemy with Sprite null for id/path '{spriteId}'");

        float gold = goldAmountField != null ? goldAmountField.value : 0f;

        // Build EnemyData with current loot items and gold amount
        return new EnemyData(
            itemName,
            desc,
            hp,
            dmg,
            spd,
            canMove,
            rng,
            spriteId,
            new List<EnemyLootItem>(currentLootItems),
            gold
        );
    }

    private void UpdateLootPreviewSprite(string itemName)
    {
        if (
            consumableItemsDatabase == null
            || lootSpritePreview == null
            || string.IsNullOrEmpty(itemName)
        )
            return;

        var allItems = consumableItemsDatabase.GetAllConsumableItems();
        var selected = allItems.Find(i => i != null && i.name == itemName);
        if (selected == null)
            return;

        string spriteId = selected.spriteId;
        if (string.IsNullOrEmpty(spriteId))
            return;

        string resolved = SpriteStorage.GetFilePathFromIdOrPath(spriteId);
        Sprite sprite = null;
        if (
            !string.IsNullOrEmpty(resolved)
            && (Path.IsPathRooted(resolved) || File.Exists(resolved))
        )
        {
            sprite = LoadSpriteFromAbsoluteFile(resolved);
        }
        else
        {
            sprite = Resources.Load<Sprite>(spriteId);
        }

        if (sprite != null)
        {
            lootSpritePreview.sprite = sprite;
            lootSpritePreview.image = sprite.texture;
        }
    }

    private void OnAddLootItemClicked()
    {
        if (itemListDropdown == null || consumableItemsDatabase == null)
            return;

        string itemName = itemListDropdown.value;
        if (string.IsNullOrEmpty(itemName))
            return;

        var allItems = consumableItemsDatabase.GetAllConsumableItems();
        var selected = allItems.Find(i => i != null && i.name == itemName);
        if (selected == null)
            return;

        float maxAmount = itemMaxAmountField != null ? itemMaxAmountField.value : 1f;
        int chance = dropChanceSlider != null ? dropChanceSlider.value : 0;

        var lootItem = new EnemyLootItem(selected.name, selected.spriteId, maxAmount, chance);
        currentLootItems.Add(lootItem);

        RefreshItemsAddedFoldoutUI();
    }

    /// <summary>
    /// Rebuilds the visual list of loot items inside the foldout based on currentLootItems.
    /// </summary>
    private void RefreshItemsAddedFoldoutUI()
    {
        if (itemsAddedFoldout == null)
            return;

        itemsAddedFoldout.Clear();
        itemsAddedFoldout.text = "Items added";

        foreach (var lootItem in currentLootItems)
        {
            if (lootItem == null)
                continue;

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.alignItems = Align.Center;

            var label = new Label(
                $"{lootItem.itemName} x{lootItem.maxAmount} ({lootItem.dropChance}%)"
            );
            var removeButton = new Button { text = "Remove" };
            removeButton.clicked += () =>
            {
                currentLootItems.Remove(lootItem);
                itemsAddedFoldout.Remove(row);
            };

            row.Add(label);
            row.Add(removeButton);
            itemsAddedFoldout.Add(row);
        }
    }

    private Sprite LoadSpriteFromAbsoluteFile(string absolutePath)
    {
        try
        {
            if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath))
                return null;
            byte[] data = File.ReadAllBytes(absolutePath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(data))
                return null;
            tex.name = Path.GetFileNameWithoutExtension(absolutePath);
            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
            sprite.name = tex.name;
            return sprite;
        }
        catch (Exception ex)
        {
            logger.Log(
                $"AddEnemyMenuController: Failed to load sprite from file '{absolutePath}'. {ex.Message}",
                this,
                Logging.LogType.Warning
            );
            return null;
        }
    }

    private string SaveSpriteFileToPersistentData(string absolutePath)
    {
        try
        {
            if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath))
                return null;

            string itemsDir = Path.Combine(Application.persistentDataPath, "Items");
            Directory.CreateDirectory(itemsDir);

            // If the file is already in persistent data, return it
            string normalized = Path.GetFullPath(absolutePath);
            if (
                normalized.StartsWith(
                    Path.GetFullPath(itemsDir),
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return normalized;
            }

            string fileName = Path.GetFileName(absolutePath);
            string destFull = Path.Combine(itemsDir, fileName);

            // Make a unique filename if collision
            destFull = GetUniqueFilePath(destFull);

            File.Copy(absolutePath, destFull);

            return destFull;
        }
        catch (Exception ex)
        {
            logger.Log(
                $"AddEnemyMenuController: Failed to copy sprite to persistent data: {ex.Message}",
                this,
                Logging.LogType.Warning
            );
            return null;
        }
    }

    private string GetUniqueFilePath(string fullPath)
    {
        string dir = Path.GetDirectoryName(fullPath);
        string name = Path.GetFileNameWithoutExtension(fullPath);
        string ext = Path.GetExtension(fullPath);
        string candidate = fullPath;
        int i = 1;
        while (File.Exists(candidate))
        {
            candidate = Path.Combine(dir, $"{name} ({i}){ext}");
            i++;
        }
        return candidate;
    }

    private void OnCloseClicked()
    {
        logger.Log("AddEnemyMenuController: Closing add enemy menu.", this);
        CloseMenu();
    }

    /*private void CloseMenu() {
        gameObject.SetActive(false);
    }*/
}
