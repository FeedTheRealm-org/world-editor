using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

public class NPCMessageItemBuilder
{
    private readonly CreatorObjectLibrarySO creatorObjectLibrary;
    private readonly Logging.Logger logger;
    private readonly Dictionary<string, string> messageQuestAssignments;

    public NPCMessageItemBuilder(
        CreatorObjectLibrarySO creatorObjectLibrary,
        Logging.Logger logger,
        Dictionary<string, string> messageQuestAssignments
    )
    {
        this.creatorObjectLibrary = creatorObjectLibrary;
        this.logger = logger;
        this.messageQuestAssignments = messageQuestAssignments;
    }

    public VisualElement CreateMessageItem(Message message)
    {
        var messageContainer = CreateMessageContainer();
        var messageLabel = CreateMessageLabel(message);
        var rightContainer = CreateRightContainer(message);

        messageContainer.Add(messageLabel);
        messageContainer.Add(rightContainer);

        return messageContainer;
    }

    private VisualElement CreateMessageContainer()
    {
        var container = new VisualElement();
        container.AddToClassList("npc-message-container");
        return container;
    }

    private Label CreateMessageLabel(Message message)
    {
        string displayContent =
            message.Content.Length > 15
                ? message.Content.Substring(0, 15) + "..."
                : message.Content;

        var label = new Label(displayContent);
        label.AddToClassList("npc-message-label");
        return label;
    }

    private VisualElement CreateRightContainer(Message message)
    {
        var rightContainer = new VisualElement();
        rightContainer.AddToClassList("npc-message-right-container");

        string currentQuestId = messageQuestAssignments.ContainsKey(message.ObjectId)
            ? messageQuestAssignments[message.ObjectId]
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

    private DropdownField CreateQuestDropdown(Message message, string currentQuestId)
    {
        var dropdown = new DropdownField();
        dropdown.AddToClassList("npc-quest-dropdown");
        dropdown.style.display = string.IsNullOrEmpty(currentQuestId)
            ? DisplayStyle.None
            : DisplayStyle.Flex;

        PopulateQuestDropdown(dropdown, currentQuestId);

        dropdown.RegisterValueChangedCallback(evt =>
        {
            var quests = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.Quest)
                .Cast<GenericQuest>()
                .ToList();
            var selectedQuest = quests.FirstOrDefault(q => q.DisplayName == evt.newValue);
            if (selectedQuest != null)
            {
                messageQuestAssignments[message.ObjectId] = selectedQuest.ObjectId;
            }
        });

        return dropdown;
    }

    private Button CreateAddQuestButton(
        Message message,
        DropdownField questDropdown,
        string currentQuestId
    )
    {
        var button = new Button();
        button.AddToClassList("npc-add-quest-button");
        button.text = "add quest";

        button.clicked += () =>
        {
            questDropdown.style.display = DisplayStyle.Flex;
            button.style.display = DisplayStyle.None;

            var rightContainer = button.parent;
            var removeButton = rightContainer?.Q<Button>();
            if (removeButton != null && removeButton.text == "✕")
            {
                removeButton.style.display = DisplayStyle.Flex;
            }

            var quests = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.Quest)
                .Cast<GenericQuest>()
                .ToList();
            var initialQuest = quests.FirstOrDefault(q => q.DisplayName == questDropdown.value);
            if (initialQuest != null)
            {
                messageQuestAssignments[message.ObjectId] = initialQuest.ObjectId;
            }
        };

        if (!string.IsNullOrEmpty(currentQuestId))
        {
            button.style.display = DisplayStyle.None;
        }

        return button;
    }

    private Button CreateRemoveQuestButton(
        Message message,
        DropdownField questDropdown,
        Button addQuestButton,
        string currentQuestId
    )
    {
        var button = new Button();
        button.AddToClassList("npc-remove-quest-button");
        button.text = "✕";

        button.clicked += () =>
        {
            if (messageQuestAssignments.ContainsKey(message.ObjectId))
            {
                messageQuestAssignments.Remove(message.ObjectId);
            }

            questDropdown.style.display = DisplayStyle.None;
            button.style.display = DisplayStyle.None;

            if (addQuestButton != null)
            {
                addQuestButton.style.display = DisplayStyle.Flex;
            }
        };

        if (string.IsNullOrEmpty(currentQuestId))
        {
            button.style.display = DisplayStyle.None;
        }

        return button;
    }

    private void PopulateQuestDropdown(DropdownField dropdown, string currentQuestId = "")
    {
        if (dropdown == null || creatorObjectLibrary == null)
            return;

        var quests = creatorObjectLibrary
            .GetCreatables(CreatorObjectCategories.Quest)
            .Cast<GenericQuest>()
            .ToList();

        dropdown.choices = quests.Select(q => q.DisplayName).ToList();

        if (!string.IsNullOrEmpty(currentQuestId))
        {
            var currentQuest = quests.FirstOrDefault(q => q.ObjectId == currentQuestId);
            if (currentQuest != null)
            {
                dropdown.value = currentQuest.DisplayName;
            }
        }
        else if (dropdown.choices.Count > 0)
        {
            dropdown.value = dropdown.choices[0];
        }
    }
}
