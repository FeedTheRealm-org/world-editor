using System;
using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FTRShared.Runtime.Models;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.EditorBar.ElementOption.NPCMenu
{
    public class NPCDialogProgressionItemBuilder
    {
        private readonly VisualTreeAsset itemTemplate;
        private readonly CreatablesManager creatablesManager;
        private readonly Action onProgressionChanged;

        public NPCDialogProgressionItemBuilder(
            VisualTreeAsset itemTemplate,
            CreatablesManager creatablesManager,
            Action onProgressionChanged
        )
        {
            this.itemTemplate = itemTemplate;
            this.creatablesManager = creatablesManager;
            this.onProgressionChanged = onProgressionChanged;
        }

        public VisualElement Build(
            NPCDialogData entry,
            int index,
            Action onRemove,
            Action onMoveUp,
            Action onMoveDown
        )
        {
            var root = itemTemplate.Instantiate();

            var itemRoot = root.Q<VisualElement>("ProgressionItemRoot");
            var toggleRow = root.Q<VisualElement>("ItemToggleRow");
            var content = root.Q<VisualElement>("ItemContent");
            var chevron = root.Q<Label>("ChevronLabel");
            var titleLabel = root.Q<Label>("ItemTitle");
            var actions = root.Q<VisualElement>("ItemActions");

            var removeBtn = root.Q<Button>("RemoveButton");
            var moveUpBtn = root.Q<Button>("MoveUpButton");
            var moveDownBtn = root.Q<Button>("MoveDownButton");

            var messagesContainer = root.Q<VisualElement>("MessagesContainer");
            var onQuestRow = root.Q<VisualElement>("OnQuestAcceptedRow");
            var onQuestDropdown = root.Q<DropdownField>("OnQuestAcceptedDropdown");
            var repeatableRow = root.Q<VisualElement>("RepeatableRow");
            var repeatableToggle = root.Q<Toggle>("RepeatableToggle");
            var cooldownDropdown = root.Q<DropdownField>("RepeatableCooldownDropdown");

            var dialog = creatablesManager
                .GetAll<Dialog>()
                .FirstOrDefault(d => d.Id == entry.dialogId);

            titleLabel.text = $"{index}. {(dialog != null ? dialog.data.name : "Unknown")}";

            bool expanded = false;

            toggleRow.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target is Button)
                    return;

                expanded = !expanded;
                content.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                chevron.text = expanded ? "▼" : "▶";

                actions.style.display = expanded ? DisplayStyle.None : DisplayStyle.Flex;
            });

            removeBtn.clicked += onRemove;
            moveUpBtn.clicked += onMoveUp;
            moveDownBtn.clicked += onMoveDown;

            var questMap = entry.GetMessageQuestMap();

            var messageBuilder = new NPCMessageItemBuilder(
                creatablesManager,
                questMap,
                () =>
                {
                    entry.SetMessageQuestMap(questMap);
                    onProgressionChanged?.Invoke();
                }
            );

            messageBuilder.BindOnQuestAcceptedElements(
                onQuestRow,
                onQuestDropdown,
                entry.onQuestAcceptedDialogId,
                newId =>
                {
                    entry.onQuestAcceptedDialogId = newId;
                    onProgressionChanged?.Invoke();
                }
            );

            messageBuilder.BindRepeatableElements(
                repeatableRow,
                repeatableToggle,
                cooldownDropdown,
                entry.repeatableQuestCooldown,
                newCooldown =>
                {
                    entry.repeatableQuestCooldown = newCooldown;
                    onProgressionChanged?.Invoke();
                }
            );

            if (dialog != null)
            {
                foreach (var message in dialog.data.messages)
                    messagesContainer.Add(messageBuilder.CreateMessageItem(message));
            }
            else
            {
                var missing = new UnityEngine.UIElements.Label("(dialog not found)");
                missing.AddToClassList("npc-message-label");
                messagesContainer.Add(missing);
            }

            return root;
        }
    }
}
