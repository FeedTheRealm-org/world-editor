using System;
using System.Collections.Generic;
using System.IO;
using Models;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

[RequireComponent(typeof(UIDocument))]
public abstract class BaseCreatorMenuController<T> : MenuController
    where T : CreatorObject
{
    [SerializeField]
    protected Logging.Logger logger;

    [SerializeField]
    protected T currentObject;

    [SerializeField]
    protected CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    protected GameObject returnMenuPrefab;

    protected TextField nameInput;
    protected Button saveButton;
    protected Button returnButton;
    protected Button closeButton;
    protected Button loadSpriteButton;
    protected Image spritePreview;
    protected string pendingSpriteSourcePath;

    private Dictionary<Button, Action> buttonCallbacks = new Dictionary<Button, Action>();

    protected abstract CreatorObjectCategories Category { get; }
    protected abstract string ObjectTypeName { get; }
    protected virtual bool RequiresSprite => false;
    protected virtual string SaveButtonName => $"Save{ObjectTypeName}";

    protected virtual void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        InitializeCommonFields(root);
        InitializeSpecificFields(root);
        RegisterCommonCallbacks();

        if (currentObject == null)
        {
            currentObject = EditContext.GetAndClearObjectToEdit<T>();
        }

        if (currentObject != null)
        {
            PopulateFields();
        }
    }

    private void InitializeCommonFields(VisualElement root)
    {
        nameInput = root.Q<TextField>("NameField");
        LogIfNull(nameInput, "Name input field");

        saveButton = root.Q<Button>(SaveButtonName) ?? root.Q<Button>("SaveButton");

        returnButton = root.Q<Button>("Return");
        closeButton = root.Q<Button>("Close");

        if (RequiresSprite)
        {
            spritePreview = root.Q<Image>("SpritePreview");

            var previewContainer =
                root.Q<VisualElement>($"{ObjectTypeName}PreviewContainer")
                ?? root.Q<VisualElement>("EnemyPreviewContainer")
                ?? root.Q<VisualElement>("NPCPreviewContainer")
                ?? root.Q<VisualElement>("ItemPreviewContainer");

            if (previewContainer != null)
            {
                loadSpriteButton = previewContainer.Q<Button>();
            }
        }
    }

    protected abstract void InitializeSpecificFields(VisualElement root);

    private void RegisterCommonCallbacks()
    {
        RegisterButtonCallback(saveButton, OnSaveClicked);
        RegisterButtonCallback(returnButton, ReturnToPreviousMenu);
        RegisterButtonCallback(closeButton, CloseMenu);

        if (loadSpriteButton != null)
        {
            RegisterButtonCallback(loadSpriteButton, LoadSprite);
        }
    }

    protected void RegisterButtonCallback(Button button, Action callback)
    {
        if (button != null)
        {
            button.clicked += callback;
            buttonCallbacks[button] = callback;
        }
    }

    protected void LogIfNull(object obj, string fieldName)
    {
        if (obj == null)
        {
            logger?.Log($"{fieldName} not found in UI", this, Logging.LogType.Warning);
        }
    }

    protected abstract void PopulateFields();

    protected virtual void OnSaveClicked()
    {
        if (!ValidateRequiredFields())
        {
            return;
        }

        if (
            RequiresSprite
            && currentObject == null
            && string.IsNullOrEmpty(pendingSpriteSourcePath)
        )
        {
            ShowValidationError($"{ObjectTypeName} sprite is required");
            return;
        }

        if (currentObject == null)
        {
            CreateNewObject();
        }
        else
        {
            UpdateExistingObject();
        }

        ShowSuccessNotification();
        ReturnToPreviousMenu();
    }

    protected virtual bool ValidateRequiredFields()
    {
        if (string.IsNullOrEmpty(nameInput?.value))
        {
            ShowValidationError($"{ObjectTypeName} name is required");
            return false;
        }
        return ValidateSpecificFields();
    }

    protected virtual bool ValidateSpecificFields() => true;

    protected abstract void CreateNewObject();
    protected abstract void UpdateExistingObject();

    protected void ShowValidationError(string message)
    {
        logger?.Log(message, this, Logging.LogType.Warning);
        ToastNotification.Show(message, "error", Color.red);
    }

    protected void ShowSuccessNotification()
    {
        ToastNotification.Show($"{ObjectTypeName} saved successfully", "success", Color.green);
    }

    protected virtual void ReturnToPreviousMenu()
    {
        if (returnMenuPrefab != null)
        {
            OpenMenu(returnMenuPrefab);
        }
        else
        {
            logger?.Log(
                $"Return menu prefab not assigned in inspector for {ObjectTypeName}. Please assign it in the Unity Inspector.",
                this,
                Logging.LogType.Error
            );
        }
    }

    protected virtual void LoadSprite()
    {
        FileHandler.ShowFilePickerDialog(
            onSuccess: OnSpriteSelected,
            onCancel: () => logger?.Log("Sprite selection canceled", this, Logging.LogType.Info)
        );
    }

    protected virtual void OnSpriteSelected(string[] paths)
    {
        if (paths == null || paths.Length == 0)
            return;

        string sourcePath = paths[0];

        if (!sourcePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            logger?.Log("Selected file is not a PNG", this, Logging.LogType.Warning);
            return;
        }

        Sprite sprite = FileHandler.LoadSpriteFromDisk(sourcePath);
        if (sprite == null)
        {
            logger?.Log("Failed to load sprite for preview", this, Logging.LogType.Error);
            return;
        }

        spritePreview.sprite = sprite;
        pendingSpriteSourcePath = sourcePath;
        logger?.Log("Sprite loaded for preview (not saved yet)", this, Logging.LogType.Info);
    }

    protected void LoadExistingSprite(string spritePath)
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

    protected virtual string SaveSpriteIfNeeded()
    {
        if (!string.IsNullOrEmpty(pendingSpriteSourcePath))
        {
            string objectId =
                currentObject != null ? currentObject.ObjectId : Guid.NewGuid().ToString();
            return FileHandler.SaveFile(pendingSpriteSourcePath, "Sprites", objectId);
        }
        return null;
    }

    protected virtual void OnDisable()
    {
        foreach (var kvp in buttonCallbacks)
        {
            if (kvp.Key != null)
            {
                kvp.Key.clicked -= kvp.Value;
            }
        }
        buttonCallbacks.Clear();
    }
}
