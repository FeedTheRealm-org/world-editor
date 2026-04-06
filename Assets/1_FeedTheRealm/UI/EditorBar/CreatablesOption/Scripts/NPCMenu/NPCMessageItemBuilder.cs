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
        private readonly CreatablesManager creatablesManager;
        private readonly Dictionary<string, string> messageQuestAssignments;

        public NPCMessageItemBuilder(
            CreatablesManager creatablesManager,
            Dictionary<string, string> messageQuestAssignments
        )
        {
            this.creatablesManager = creatablesManager;
            this.messageQuestAssignments = messageQuestAssignments;
        }

        public VisualElement CreateMessageItem(MessageData message)
        {
            var container = new VisualElement();
            container.AddToClassList("npc-message-container");

            string displayContent =
                message.content.Length > 15
                    ? message.content.Substring(0, 15) + "..."
                    : message.content;

            var label = new Label(displayContent);
            label.AddToClassList("npc-message-label");

            var rightContainer = CreateRightContainer(message);

            container.Add(label);
            container.Add(rightContainer);
            return container;
        }

        private VisualElement CreateRightContainer(MessageData message)
        {
            var rightContainer = new VisualElement();
            rightContainer.AddToClassList("npc-message-right-container");

            string currentQuestId = messageQuestAssignments.ContainsKey(message.id)
                ? messageQuestAssignments[message.id]
                : "";

            var questDropdown = CreateQuestDropdown(message, currentQuestId);
            var addQuestButton = CreateAddQuestButton(message, questDropdown, currentQuestId);
            var removeQuestButton = CreateRemoveQuestButton(
                message,
                questDropdown,
                addQuestButton,
                currentQuestId
            );

            rightContainer.Add(questDropdown);
            rightContainer.Add(removeQuestButton);
            rightContainer.Add(addQuestButton);
            return rightContainer;
        }

        private DropdownField CreateQuestDropdown(MessageData message, string currentQuestId)
        {
            var dropdown = new DropdownField();
            dropdown.AddToClassList("npc-quest-dropdown");
            dropdown.style.display = string.IsNullOrEmpty(currentQuestId)
                ? DisplayStyle.None
                : DisplayStyle.Flex;

            PopulateQuestDropdown(dropdown, currentQuestId);

            dropdown.RegisterValueChangedCallback(evt =>
            {
                var selected = creatablesManager
                    .GetAll<Quest>()
                    .FirstOrDefault(q => q.data.title == evt.newValue);
                if (selected != null)
                    messageQuestAssignments[message.id] = selected.Id;
            });

            return dropdown;
        }

        private Button CreateAddQuestButton(
            MessageData message,
            DropdownField questDropdown,
            string currentQuestId
        )
        {
            var button = new Button { text = "add quest" };
            button.AddToClassList("npc-add-quest-button");

            button.clicked += () =>
            {
                questDropdown.style.display = DisplayStyle.Flex;
                button.style.display = DisplayStyle.None;

                var removeButton = button.parent?.Q<Button>();
                if (removeButton != null && removeButton.text == "✕")
                    removeButton.style.display = DisplayStyle.Flex;

                var initial = creatablesManager
                    .GetAll<Quest>()
                    .FirstOrDefault(q => q.data.title == questDropdown.value);
                if (initial != null)
                    messageQuestAssignments[message.id] = initial.Id;
            };

            if (!string.IsNullOrEmpty(currentQuestId))
                button.style.display = DisplayStyle.None;

            return button;
        }

        private Button CreateRemoveQuestButton(
            MessageData message,
            DropdownField questDropdown,
            Button addQuestButton,
            string currentQuestId
        )
        {
            var button = new Button { text = "✕" };
            button.AddToClassList("npc-remove-quest-button");

            button.clicked += () =>
            {
                messageQuestAssignments.Remove(message.id);
                questDropdown.style.display = DisplayStyle.None;
                button.style.display = DisplayStyle.None;
                addQuestButton.style.display = DisplayStyle.Flex;
            };

            if (string.IsNullOrEmpty(currentQuestId))
                button.style.display = DisplayStyle.None;

            return button;
        }

        private void PopulateQuestDropdown(DropdownField dropdown, string currentQuestId = "")
        {
            var quests = creatablesManager.GetAll<Quest>();
            dropdown.choices = quests.Select(q => q.data.title).ToList();

            if (!string.IsNullOrEmpty(currentQuestId))
            {
                var current = quests.FirstOrDefault(q => q.Id == currentQuestId);
                if (current != null)
                    dropdown.value = current.data.title;
            }
            else if (dropdown.choices.Count > 0)
                dropdown.value = dropdown.choices[0];
        }
    }
}
