using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class AddItemMenuController : MonoBehaviour {
    [SerializeField] private Maker player;

    [Serializable]
    public class ConsumableData {
        public string name;
        public string description;
        public string effectType; // e.g., Heal, Buff, Damage, Speed, Mana, Custom
        public int value; // magnitude of effect
        public float duration; // seconds; 0 if instant
        public float cooldown; // seconds
        public int maxStack; // default 1
        public Sprite sprite;
    }

    [Serializable]
    public class ItemAddedEvent : UnityEvent<ConsumableData> { }

    [Header("Events")]
    public ItemAddedEvent OnItemAdded;

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
        var consumable = BuildConsumableFromUI();
        if (consumable == null) return;

        Debug.Log($"AddItemMenuController: Adding consumable '{consumable.name}' (Effect {consumable.effectType}, Value {consumable.value}, Duration {consumable.duration}s)");

        OnItemAdded?.Invoke(consumable);
        CloseMenu();
    }

    private void OnLoadSpriteClicked() {
        Sprite sprite = LoadSpriteFromPath(spritePathInput?.value);
        if (sprite != null) {
            // UI Toolkit Image supports sprite in modern versions; also set image for compatibility
            spritePreview.sprite = sprite;
            spritePreview.image = sprite.texture;
        } else {
            Debug.LogWarning("AddItemMenuController: Could not load sprite. Ensure it is under Resources and path has no extension.");
        }
    }

    private ConsumableData BuildConsumableFromUI() {
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

        return new ConsumableData {
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
