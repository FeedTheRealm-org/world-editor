using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FeedTheRealm.Core.WorldObjects.NPCs;
using FeedTheRealm.Gameplay.Library.CreatorObjectLibrary;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class NPCsMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private GameObject createNPCMenuPrefab;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private VisualTreeAsset itemListTemplate;
    private Button closeButton;
    private Button addNPCButton;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        closeButton = root.Q<Button>("Close");
        addNPCButton = root.Q<Button>("AddNPC");

        addNPCButton.clicked += AddNPC;
        closeButton.clicked += CloseMenu;

        PopulateNPCsList();
    }

    private void PopulateNPCsList()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var npcsList = root.Q<ListView>("NPCList");

        if (npcsList == null)
        {
            logger?.Log("NPCList ListView not found in UI", this, Logging.LogType.Error);
            return;
        }

        npcsList.Clear();

        if (itemListTemplate == null)
        {
            logger?.Log("itemListTemplate is not assigned", this, Logging.LogType.Error);
            return;
        }

        foreach (GenericNPC npc in creatorObjectLibrary.GetCreatables(CreatorObjectCategories.NPC))
        {
            VisualElement npcEntry = itemListTemplate.Instantiate();
            var headerLabel = npcEntry.Q<Label>("Header");
            if (headerLabel != null)
            {
                headerLabel.text = npc.DisplayName;
            }

            var editButton = npcEntry.Q<Button>("Edit");
            var deleteButton = npcEntry.Q<Button>("Delete");

            var typeLabel = npcEntry.Q<Label>("Type");
            if (typeLabel != null)
                typeLabel.text = "NPC";

            if (editButton != null)
                editButton.clicked += () => OnEditNPC(npc);
            if (deleteButton != null)
                deleteButton.clicked += () => OnDeleteNPC(npc, npcEntry);

            npcsList.hierarchy.Add(npcEntry);
        }
    }

    void OnEditNPC(CreatorObject npc)
    {
        logger.Log("Editing NPC: " + npc.DisplayName, this, Logging.LogType.Info);

        EditContext.SetObjectToEdit(npc);

        OpenMenu(createNPCMenuPrefab);
    }

    void OnDeleteNPC(CreatorObject npc, VisualElement npcListEntry)
    {
        logger.Log("Deleting NPC: " + npc.DisplayName, this, Logging.LogType.Info);
        creatorObjectLibrary.RemoveCreatable(CreatorObjectCategories.NPC, npc);
        npcListEntry.RemoveFromHierarchy();
    }

    void OnDisable()
    {
        addNPCButton.clicked -= AddNPC;
        closeButton.clicked -= CloseMenu;
    }

    private void AddNPC()
    {
        logger.Log("Opening Create NPC Menu", this, Logging.LogType.Info);
        OpenMenu(createNPCMenuPrefab);
    }
}
