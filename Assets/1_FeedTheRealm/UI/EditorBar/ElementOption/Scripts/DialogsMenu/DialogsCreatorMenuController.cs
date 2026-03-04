using System;
using FeedTheRealm.Core.WorldObjects.Dialogs;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class DialogsCreatorMenuController : BaseCreatorMenuController<Dialog>
{
    protected override CreatorObjectCategories Category => CreatorObjectCategories.Dialog;
    protected override string ObjectTypeName => "Dialog";
    protected override string SaveButtonName => "SaveButton";

    protected override void InitializeSpecificFields(VisualElement root)
    {
        // No additional fields needed for Dialog
    }

    protected override void PopulateFields()
    {
        nameInput.value = currentObject.name;
    }

    protected override void CreateNewObject()
    {
        var dialogData = new DialogData("", nameInput.value);
        currentObject = new Dialog(dialogData);
        creatorObjectLibrary.AddCreatable(Category, currentObject);
        logger?.Log($"Created new dialog: {currentObject.name}", this, Logging.LogType.Info);
    }

    protected override void UpdateExistingObject()
    {
        currentObject.name = nameInput.value;
        logger?.Log($"Updated dialog: {currentObject.name}", this, Logging.LogType.Info);
    }
}
