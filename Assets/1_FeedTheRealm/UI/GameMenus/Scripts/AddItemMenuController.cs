using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class AddItemMenuController : MonoBehaviour {
    [SerializeField] private Maker player;
    [SerializeField] private ConsumableItems consumableItemsDatabase;

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

    private void OnEnable() {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        if (root == null) {
            Debug.LogError("AddItemMenuController: UIDocument has no visual tree. Assign a UXML to the Source Asset.");
            return;
        }

        if (player != null) {
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

        if (effectTypeDropdown != null) {
            effectTypeDropdown.choices = new List<string> { "Heal", "Buff", "Damage", "Speed", "Mana", "Custom" };
            if (string.IsNullOrEmpty(effectTypeDropdown.value))
                effectTypeDropdown.value = "Heal";
        }

        if (addButton != null) addButton.clicked += OnAddClicked;
        if (closeButton != null) closeButton.clicked += OnCloseClicked;
        if (loadSpriteButton != null) loadSpriteButton.clicked += OnLoadSpriteClicked;
    }

    private void OnDisable() {
        if (addButton != null) addButton.clicked -= OnAddClicked;
        if (closeButton != null) closeButton.clicked -= OnCloseClicked;
        if (loadSpriteButton != null) loadSpriteButton.clicked -= OnLoadSpriteClicked;
    }

    private void OnAddClicked() {
        if (consumableItemsDatabase == null) {
            Debug.LogError("AddItemMenuController: consumableItemsDatabase is not assigned. Assign it in the Inspector.");
            return;
        }

        var consumable = BuildConsumableFromUI();
        if (consumable == null) return;

        Debug.Log($"AddItemMenuController: Adding consumable '{consumable.name}' (Effect {consumable.effectType}, Value {consumable.value}, Duration {consumable.duration}s)");

        try {
            consumableItemsDatabase.AddConsumableItem(consumable);
        } catch (Exception ex) {
            Debug.LogError($"AddItemMenuController: Failed to add consumable to database: {ex.Message}");
            return;
        }

        CloseMenu();
    }

    private void OnLoadSpriteClicked() {
        // In the Editor, allow picking a file from disk; otherwise fall back to Resources path loading
#if UNITY_EDITOR
        string startDir = Application.dataPath;
        string selected = UnityEditor.EditorUtility.OpenFilePanel("Select Sprite", startDir, "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(selected)) {
            // Try to compute a Resources-relative path (no extension) if inside a Resources folder
            string resourcesPath = TryMakeResourcesPath(selected);
            if (!string.IsNullOrEmpty(resourcesPath) && spritePathInput != null) {
                spritePathInput.value = resourcesPath;
            }

            // Preview from the absolute file path to give immediate feedback
            var previewSprite = LoadSpriteFromAbsoluteFile(selected);
            if (previewSprite != null) {
                spritePreview.sprite = previewSprite;
                spritePreview.image = previewSprite.texture;
                return;
            }
        }
#endif

        // Fallback to existing behavior using Resources path typed in the input
        Sprite sprite = LoadSpriteFromPath(spritePathInput?.value);
        if (sprite != null) {
            spritePreview.sprite = sprite;
            spritePreview.image = sprite.texture;
        } else {
            Debug.LogWarning("AddItemMenuController: Could not load sprite. Ensure it is under Resources and path has no extension.");
        }
    }

    private ConsumableItems.ConsumableData BuildConsumableFromUI() {
        string itemName = nameInput != null ? nameInput.value?.Trim() : string.Empty;
        if (string.IsNullOrEmpty(itemName)) {
            Debug.LogError("Item name cannot be empty.");
            return null;
        }
        string desc = descriptionInput != null ? descriptionInput.value?.Trim() : string.Empty;
        string effect = effectTypeDropdown != null ? effectTypeDropdown.value : "Heal";
        int val = valueField != null ? valueField.value : 0;
        float dur = durationField != null ? durationField.value : 0f;
        float cd = cooldownField != null ? cooldownField.value : 0f;
        int stack = Mathf.Max(1, maxStackField != null ? maxStackField.value : 1);

        Sprite sprite = LoadSpriteFromPath(spritePathInput != null ? spritePathInput.value : null);
        if (sprite != null && spritePreview != null) {
            spritePreview.sprite = sprite;
            spritePreview.image = sprite.texture;
        }

        return new ConsumableItems.ConsumableData {
            name = itemName,
            description = desc,
            effectType = effect,
            value = val,
            duration = dur,
            cooldown = cd,
            maxStack = stack,
            sprite = sprite
        };
    }

    private Sprite LoadSpriteFromPath(string path) {
        if (string.IsNullOrWhiteSpace(path)) return null;
        // Path should be relative to a Resources folder and without extension
        var sprite = Resources.Load<Sprite>(path.Trim());
        return sprite;
    }

    private Sprite LoadSpriteFromAbsoluteFile(string absolutePath) {
        try {
            if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath)) return null;
            byte[] data = File.ReadAllBytes(absolutePath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(data)) return null;
            tex.name = Path.GetFileNameWithoutExtension(absolutePath);
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = tex.name;
            return sprite;
        } catch (Exception ex) {
            Debug.LogWarning($"AddItemMenuController: Failed to load sprite from file '{absolutePath}'. {ex.Message}");
            return null;
        }
    }

    private string TryMakeResourcesPath(string absolutePath) {
        // If file is under any 'Resources' folder within the project, return the path relative to that folder without extension
        try {
            if (string.IsNullOrEmpty(absolutePath)) return null;
            string projectPath = Application.dataPath.Replace("/", Path.DirectorySeparatorChar.ToString());
            string normalized = absolutePath.Replace("/", Path.DirectorySeparatorChar.ToString());
            if (!normalized.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase)) return null;

            // Find "/Resources/" segment
            int idx = normalized.IndexOf(Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            int start = idx + ("" + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar).Length;
            string rel = normalized.Substring(start);
            string withoutExt = Path.ChangeExtension(rel, null);
            // Convert back to Unity-style forward slashes
            return withoutExt.Replace(Path.DirectorySeparatorChar, '/');
        } catch {
            return null;
        }
    }

    private void OnCloseClicked() {
        Debug.Log("AddItemMenuController: Closing add item menu.");
        CloseMenu();
    }

    private void CloseMenu() {
        if (player != null) {
            player.ToggleMovement(true);
        }
        gameObject.SetActive(false);
    }
}
