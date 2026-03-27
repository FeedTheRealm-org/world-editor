// using System;
// using FeedTheRealm.Core.Library;
// using FeedTheRealm.UI.EditorBar.ElementOption.Base;
// using FTRShared.Runtime.Models;
// using UnityEngine;
// using UnityEngine.UIElements;

// namespace FeedTheRealm.UI.EditorBar.ElementOption.DialogsMenu
// {
//     [RequireComponent(typeof(UIDocument))]
//     public class DialogsCreatorMenuController : BaseCreatorMenuController<Dialog>
//     {
//         protected override CreatableObjectCategories Category => CreatableObjectCategories.Dialog;
//         protected override string ObjectTypeName => "Dialog";
//         protected override string SaveButtonName => "SaveButton";

//         protected override void InitializeSpecificFields(VisualElement root)
//         {
//             // No additional fields needed for Dialog
//         }

//         protected override void PopulateFields()
//         {
//             nameInput.value = currentObject.name;
//         }

//         protected override void CreateNewObject()
//         {
//             var dialogData = new DialogData("", nameInput.value);
//             currentObject = new Dialog(dialogData);
//             creatorObjectLibrary.AddCreatable(Category, currentObject);
//             logger?.Log($"Created new dialog: {currentObject.name}", this, Logging.LogType.Info);
//         }

//         protected override void UpdateExistingObject()
//         {
//             currentObject.name = nameInput.value;
//             logger?.Log($"Updated dialog: {currentObject.name}", this, Logging.LogType.Info);
//         }
//     }
// }
