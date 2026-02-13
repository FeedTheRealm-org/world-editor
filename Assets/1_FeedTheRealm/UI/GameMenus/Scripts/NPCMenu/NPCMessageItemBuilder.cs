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
        container.style.flexDirection = FlexDirection.Row;
        container.style.justifyContent = Justify.SpaceBetween;
        container.style.alignItems = Align.Center;
        container.style.marginBottom = 5;
        container.style.paddingLeft = 5;
        container.style.paddingRight = 5;
        container.style.paddingTop = 3;
        container.style.paddingBottom = 3;
        container.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        container.style.borderBottomLeftRadius = 4;
        container.style.borderBottomRightRadius = 4;
        container.style.borderTopLeftRadius = 4;
        container.style.borderTopRightRadius = 4;
        return container;
    }

    private Label CreateMessageLabel(Message message)
    {
        string displayContent =
            message.Content.Length > 15
                ? message.Content.Substring(0, 15) + "..."
                : message.Content;

        var label = new Label(displayContent);
        label.style.color = Color.white;
        label.style.flexGrow = 1;
        label.style.fontSize = 12;
        return label;
    }

    private VisualElement CreateRightContainer(Message message)
    {
        var rightContainer = new VisualElement();
        rightContainer.style.flexDirection = FlexDirection.Row;
        rightContainer.style.alignItems = Align.Center;

        string currentQuestId = messageQuestAssignments.ContainsKey(message.ObjectId)
            ? messageQuestAssignments[message.ObjectId]
            : "";

        var questDropdown = CreateQuestDropdown(message, currentQuestId);
        var addQuestButton = CreateAddQuestButton(message, questDropdown, currentQuestId);

        rightContainer.Add(questDropdown);
        rightContainer.Add(addQuestButton);

        return rightContainer;
    }

    private DropdownField CreateQuestDropdown(Message message, string currentQuestId)
    {
        var dropdown = new DropdownField();
        dropdown.style.display = string.IsNullOrEmpty(currentQuestId)
            ? DisplayStyle.None
            : DisplayStyle.Flex;
        dropdown.style.minWidth = 120;

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
        button.text = "add quest";
        button.style.fontSize = 10;
        button.style.paddingLeft = 8;
        button.style.paddingRight = 8;
        button.style.paddingTop = 4;
        button.style.paddingBottom = 4;

        button.clicked += () =>
        {
            questDropdown.style.display = DisplayStyle.Flex;
            button.style.display = DisplayStyle.None;

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
