using System;
using System.Collections.Generic;
using System.IO;
using Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class AddItemMenuController : MonoBehaviour
{
    [SerializeField]
    private Maker player;

    [SerializeField]
    private ConsumableItems consumableItemsDatabase;

    [SerializeField]
    private Logging.Logger logger;

    private Button addButton;
    private Button closeButton;
    private Button loadSpriteButton;
    private TextField nameInput;
    private TextField descriptionInput;
    private DropdownField effectTypeDropdown;
    private IntegerField valueField;
    private FloatField durationField;
    private FloatField cooldownField;
    private IntegerField maxStackField;
    private TextField spritePathInput;
    private Image spritePreview;
    private VisualElement root;
    private Label nameErrorLabel;

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

        if (player != null)
        {
            player.ToggleMovement(false);
        }

        addButton = root.Q<Button>("Add");
        closeButton = root.Q<Button>("Close");
        loadSpriteButton = root.Q<Button>("LoadSprite");

        nameInput = root.Q<TextField>("NameInput");
        descriptionInput = root.Q<TextField>("Description");
        effectTypeDropdown = root.Q<DropdownField>("EffectType");
        valueField = root.Q<IntegerField>("Value");
        durationField = root.Q<FloatField>("Duration");
        cooldownField = root.Q<FloatField>("Cooldown");
        maxStackField = root.Q<IntegerField>("MaxStack");
        spritePathInput = root.Q<TextField>("SpritePath");
        spritePreview = root.Q<Image>("SpritePreview");

        if (effectTypeDropdown != null)
        {
            effectTypeDropdown.choices = new List<string>
            {
                "Heal",
                "Buff",
                "Damage",
                "Speed",
                "Mana",
                "Custom",
            };
            if (string.IsNullOrEmpty(effectTypeDropdown.value))
                effectTypeDropdown.value = "Heal";
        }

        if (addButton != null)
            addButton.clicked += OnAddClicked;
        if (closeButton != null)
            closeButton.clicked += OnCloseClicked;
        if (loadSpriteButton != null)
            loadSpriteButton.clicked += OnLoadSpriteClicked;
    }

    private void OnDisable()
    {
        if (addButton != null)
            addButton.clicked -= OnAddClicked;
        if (closeButton != null)
            closeButton.clicked -= OnCloseClicked;
        if (loadSpriteButton != null)
            loadSpriteButton.clicked -= OnLoadSpriteClicked;
    }

    private void OnAddClicked()
    {
        if (consumableItemsDatabase == null)
        {
            logger.Log(
                "AddItemMenuController: consumableItemsDatabase is not assigned. Assign it in the Inspector.",
                this,
                Logging.LogType.Error
            );
            return;
        }

        if (!ValidateName())
            return;

        var consumable = BuildConsumableFromUI();
        if (consumable == null)
            return;

        logger.Log(
            $"AddItemMenuController: Adding consumable '{consumable.name}' (Effect {consumable.effectType}, Value {consumable.value}, Duration {consumable.duration}s)",
            this
        );

        try
        {
            consumableItemsDatabase.AddConsumableItem(consumable);
        }
        catch (Exception ex)
        {
            logger.Log(
                $"AddItemMenuController: Failed to add consumable to database: {ex.Message}",
                this,
                Logging.LogType.Error
            );
            return;
        }

        CloseMenu();
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
        logger.Log("Item name cannot be empty.", this, Logging.LogType.Error);

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
#if UNITY_EDITOR
        string startDir = Application.dataPath;
        string selected = UnityEditor.EditorUtility.OpenFilePanel(
            "Select Sprite",
            startDir,
            "png,jpg,jpeg"
        );
        if (!string.IsNullOrEmpty(selected))
        {
            Debug.Log($"AddItemMenuController: Selected sprite file '{selected}'");
            if (!string.IsNullOrEmpty(selected) && spritePathInput != null)
            {
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
                    return;
                }
            }
        }
#endif
    }

    private ConsumableItem BuildConsumableFromUI()
    {
        string itemName = nameInput != null ? nameInput.value?.Trim() : string.Empty;
        if (string.IsNullOrEmpty(itemName))
        {
            logger.Log("Item name cannot be empty.", this, Logging.LogType.Error);
            return null;
        }

        string desc = descriptionInput != null ? descriptionInput.value?.Trim() : string.Empty;
        string effect = effectTypeDropdown != null ? effectTypeDropdown.value : "Heal";
        int val = valueField != null ? valueField.value : 0;
        float dur = durationField != null ? durationField.value : 0f;
        float cd = cooldownField != null ? cooldownField.value : 0f;
        int stack = Mathf.Max(1, maxStackField != null ? maxStackField.value : 1);

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
            Debug.Log($"Try add Item with Sprite='{sprite.name}'");
        else
            Debug.LogWarning($"Try add Item with Sprite null for id/path '{spriteId}'");

        return new ConsumableItem(itemName, desc, effect, val, dur, cd, stack, spriteId);
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
                $"AddItemMenuController: Failed to load sprite from file '{absolutePath}'. {ex.Message}",
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
                $"AddItemMenuController: Failed to copy sprite to persistent data: {ex.Message}",
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
        logger.Log("AddItemMenuController: Closing add item menu.", this);
        CloseMenu();
    }

    private void CloseMenu()
    {
        if (player != null)
        {
            player.ToggleMovement(true);
        }
        gameObject.SetActive(false);
    }
}
