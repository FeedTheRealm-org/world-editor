using System;
using System.Linq;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MessagesCreatorMenuController : BaseCreatorMenuController<Message>
{
    public static string PendingDialogId;

    private TextField contentField;

    protected override CreatorObjectCategories Category => CreatorObjectCategories.Message;
    protected override string ObjectTypeName => "Message";
    protected override string SaveButtonName => "SaveButton";

    protected override void OnEnable()
    {
        base.OnEnable();

        // Handle PendingDialogId for messages
        if (currentObject != null && string.IsNullOrEmpty(PendingDialogId))
        {
            PendingDialogId = currentObject.dialogId;
        }
    }

    protected override void InitializeSpecificFields(VisualElement root)
    {
        contentField = root.Q<TextField>("ContentField");
        LogIfNull(contentField, "ContentField");
    }

    protected override void PopulateFields()
    {
        contentField.value = currentObject.Content;
    }

    protected override bool ValidateRequiredFields()
    {
        if (string.IsNullOrEmpty(contentField?.value))
        {
            ShowValidationError("Message content is required");
            return false;
        }
        return ValidateSpecificFields();
    }

    protected override bool ValidateSpecificFields()
    {
        var dialogs = creatorObjectLibrary.GetCreatables(CreatorObjectCategories.Dialog);
        var dialog = dialogs.Find(d => d.ObjectId == PendingDialogId) as Dialog;

        if (dialog == null)
        {
            ShowValidationError("No dialog selected to attach message to");
            return false;
        }
        return true;
    }

    protected override void CreateNewObject()
    {
        var dialogs = creatorObjectLibrary.GetCreatables(CreatorObjectCategories.Dialog);
        var dialog = dialogs.Find(d => d.ObjectId == PendingDialogId) as Dialog;

        currentObject = new Message("", "", contentField.value, dialog.ObjectId);
        creatorObjectLibrary.AddCreatable(Category, currentObject);
        logger?.Log(
            $"Created new message for dialog {dialog.DisplayName}",
            this,
            Logging.LogType.Info
        );
    }

    protected override void UpdateExistingObject()
    {
        var dialogs = creatorObjectLibrary.GetCreatables(CreatorObjectCategories.Dialog);
        var dialog = dialogs.Find(d => d.ObjectId == PendingDialogId) as Dialog;

        currentObject.Content = contentField.value;
        currentObject.dialogId = dialog.ObjectId;
        logger?.Log($"Updated message: {currentObject.DisplayName}", this, Logging.LogType.Info);
    }
}
