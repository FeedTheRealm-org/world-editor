using System;
using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FTRShared.Runtime.Models;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.EditorBar.ElementOption.NPCMenu
{
    public class NPCMessageItemBuilder
    {
        private static readonly List<string> CooldownChoices = new List<string>
        {
            "1 day",
            "1 week",
            "1 month",
        };

        private readonly CreatablesManager creatablesManager;
        private readonly Dictionary<string, string> messageQuestAssignments;
        private readonly Action onAssignmentsChanged;

        private VisualElement onQuestAcceptedRow;
        private DropdownField onQuestAcceptedDropdown;
        private VisualElement repeatableRow;
        private Toggle repeatableToggle;
        private DropdownField cooldownDropdown;

        private class MessageUIElements
        {
            public MessageData Message;
            public DropdownField QuestDropdown;
            public Button AddQuestButton;
            public Button RemoveQuestButton;
        }

        private readonly List<MessageUIElements> createdElements = new List<MessageUIElements>();

        public NPCMessageItemBuilder(
            CreatablesManager creatablesManager,
            Dictionary<string, string> messageQuestAssignments,
            Action onAssignmentsChanged = null
        )
        {
            this.creatablesManager = creatablesManager;
            this.messageQuestAssignments = messageQuestAssignments;
            this.onAssignmentsChanged = onAssignmentsChanged;
        }

        public void BindOnQuestAcceptedElements(
            VisualElement row,
            DropdownField dropdown,
            string currentOnQuestAcceptedDialogId,
            Action<string> onValueChanged
        )
        {
            onQuestAcceptedRow = row;
            onQuestAcceptedDropdown = dropdown;

            PopulateOnQuestAcceptedDropdown(currentOnQuestAcceptedDialogId);

            dropdown.RegisterValueChangedCallback(evt =>
            {
                var selected = creatablesManager
                    .GetAll<Dialog>()
                    .FirstOrDefault(d => d.data.name == evt.newValue);
                onValueChanged?.Invoke(selected?.Id ?? "");
            });

            RefreshExtraRowVisibility();
        }

        public void BindRepeatableElements(
            VisualElement row,
            Toggle toggle,
            DropdownField cooldown,
            string currentCooldown,
            Action<string> onValueChanged
        )
        {
            repeatableRow = row;
            repeatableToggle = toggle;
            this.cooldownDropdown = cooldown;

            // Populate cooldown choices
            cooldown.choices = CooldownChoices;

            bool isRepeatable = !string.IsNullOrEmpty(currentCooldown);
            toggle.SetValueWithoutNotify(isRepeatable);

            if (isRepeatable && CooldownChoices.Contains(currentCooldown))
                cooldown.SetValueWithoutNotify(currentCooldown);
            else
                cooldown.SetValueWithoutNotify(CooldownChoices[0]);

            cooldown.style.display = isRepeatable ? DisplayStyle.Flex : DisplayStyle.None;

            toggle.RegisterValueChangedCallback(evt =>
            {
                cooldown.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                string newCooldown = evt.newValue ? cooldown.value : "";
                onValueChanged?.Invoke(newCooldown);
            });

            cooldown.RegisterValueChangedCallback(evt => onValueChanged?.Invoke(evt.newValue));

            RefreshExtraRowVisibility();
        }

        public void ClearElements() => createdElements.Clear();

        public VisualElement CreateMessageItem(MessageData message)
        {
            var container = new VisualElement();
            container.AddToClassList("npc-message-container");

            string display =
                message.content.Length > 15
                    ? message.content.Substring(0, 15) + "..."
                    : message.content;

            var label = new Label(display);
            label.AddToClassList("npc-message-label");

            container.Add(label);
            container.Add(CreateRightContainer(message));
            return container;
        }

        private VisualElement CreateRightContainer(MessageData message)
        {
            var right = new VisualElement();
            right.AddToClassList("npc-message-right-container");

            string currentQuestId = messageQuestAssignments.TryGetValue(message.id, out var qid)
                ? qid
                : "";

            var ui = new MessageUIElements { Message = message };
            ui.QuestDropdown = CreateQuestDropdown(message, currentQuestId);
            ui.AddQuestButton = CreateAddQuestButton(message, ui.QuestDropdown, currentQuestId);
            ui.RemoveQuestButton = CreateRemoveQuestButton(message, currentQuestId);

            createdElements.Add(ui);

            right.Add(ui.QuestDropdown);
            right.Add(ui.RemoveQuestButton);
            right.Add(ui.AddQuestButton);
            return right;
        }

        private DropdownField CreateQuestDropdown(MessageData message, string currentQuestId)
        {
            var dd = new DropdownField();
            dd.AddToClassList("npc-quest-dropdown");
            dd.style.display = string.IsNullOrEmpty(currentQuestId)
                ? DisplayStyle.None
                : DisplayStyle.Flex;

            PopulateQuestDropdown(dd, currentQuestId);

            dd.RegisterValueChangedCallback(evt =>
            {
                messageQuestAssignments.Clear();
                var selected = creatablesManager
                    .GetAll<Quest>()
                    .FirstOrDefault(q => q.data.title == evt.newValue);
                if (selected != null)
                    messageQuestAssignments[message.id] = selected.Id;

                RefreshMessageVisibilities();
                onAssignmentsChanged?.Invoke();
            });

            return dd;
        }

        private Button CreateAddQuestButton(
            MessageData message,
            DropdownField questDropdown,
            string currentQuestId
        )
        {
            var btn = new Button { text = "set quest" };
            btn.AddToClassList("npc-add-quest-button");

            btn.clicked += () =>
            {
                messageQuestAssignments.Clear();

                var picked =
                    creatablesManager
                        .GetAll<Quest>()
                        .FirstOrDefault(q => q.data.title == questDropdown.value)
                    ?? creatablesManager.GetAll<Quest>().FirstOrDefault();

                if (picked != null)
                    messageQuestAssignments[message.id] = picked.Id;

                RefreshMessageVisibilities();
                onAssignmentsChanged?.Invoke();
            };

            if (!string.IsNullOrEmpty(currentQuestId))
                btn.style.display = DisplayStyle.None;

            return btn;
        }

        private Button CreateRemoveQuestButton(MessageData message, string currentQuestId)
        {
            var btn = new Button { text = "✕" };
            btn.AddToClassList("npc-remove-quest-button");

            btn.clicked += () =>
            {
                messageQuestAssignments.Remove(message.id);
                RefreshMessageVisibilities();
                onAssignmentsChanged?.Invoke();
            };

            if (string.IsNullOrEmpty(currentQuestId))
                btn.style.display = DisplayStyle.None;

            return btn;
        }

        private void PopulateQuestDropdown(DropdownField dd, string currentQuestId)
        {
            var quests = creatablesManager.GetAll<Quest>();
            dd.choices = quests.Select(q => q.data.title).ToList();

            if (!string.IsNullOrEmpty(currentQuestId))
            {
                var current = quests.FirstOrDefault(q => q.Id == currentQuestId);
                if (current != null)
                    dd.SetValueWithoutNotify(current.data.title);
            }
            else if (dd.choices.Count > 0)
                dd.SetValueWithoutNotify(dd.choices[0]);
        }

        private void PopulateOnQuestAcceptedDropdown(string currentDialogId)
        {
            if (onQuestAcceptedDropdown == null)
                return;

            var dialogs = creatablesManager.GetAll<Dialog>();
            onQuestAcceptedDropdown.choices = new List<string> { "None" }
                .Concat(dialogs.Select(d => d.data.name))
                .ToList();

            if (!string.IsNullOrEmpty(currentDialogId))
            {
                var current = dialogs.FirstOrDefault(d => d.Id == currentDialogId);
                onQuestAcceptedDropdown.SetValueWithoutNotify(
                    current != null ? current.data.name : "None"
                );
            }
            else
            {
                onQuestAcceptedDropdown.SetValueWithoutNotify("None");
            }
        }

        private void RefreshMessageVisibilities()
        {
            createdElements.RemoveAll(e =>
                e.QuestDropdown.parent == null && e.AddQuestButton.parent == null
            );

            foreach (var item in createdElements)
            {
                bool hasQuest =
                    messageQuestAssignments.TryGetValue(item.Message.id, out var qid)
                    && !string.IsNullOrEmpty(qid);

                item.AddQuestButton.style.display = hasQuest
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
                item.RemoveQuestButton.style.display = hasQuest
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
                item.QuestDropdown.style.display = hasQuest ? DisplayStyle.Flex : DisplayStyle.None;

                if (hasQuest)
                {
                    var q = creatablesManager.GetAll<Quest>().FirstOrDefault(x => x.Id == qid);
                    if (q != null)
                        item.QuestDropdown.SetValueWithoutNotify(q.data.title);
                }
            }

            RefreshExtraRowVisibility();
        }

        private void RefreshExtraRowVisibility()
        {
            bool hasQuest = messageQuestAssignments.Count > 0;

            if (onQuestAcceptedRow != null)
                onQuestAcceptedRow.style.display = hasQuest ? DisplayStyle.Flex : DisplayStyle.None;

            if (repeatableRow != null)
                repeatableRow.style.display = hasQuest ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
