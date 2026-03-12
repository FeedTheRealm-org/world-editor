using System;
using FeedTheRealm.Core.WorldObjects.Items;
using FeedTheRealm.UI.EditorBar.ElementOption.Base;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace FeedTheRealm.UI.EditorBar.ElementOption.ItemsMenu
{
    [RequireComponent(typeof(UIDocument))]
    public abstract class ItemCreatorMenuController<TItem> : BaseCreatorMenuController<TItem>
        where TItem : Item
    {
        protected TextField descriptionInput;

        protected override bool RequiresSprite => true;

        protected override void InitializeSpecificFields(VisualElement root)
        {
            descriptionInput = root.Q<TextField>("DescriptionField");
            LogIfNull(descriptionInput, "Description input field");

            InitializeItemSpecificFields(root);
        }

        protected abstract void InitializeItemSpecificFields(VisualElement root);

        protected void ReturnToItemsMenu()
        {
            if (returnMenuPrefab != null)
            {
                OpenMenu(returnMenuPrefab);
            }
        }
    }
}
