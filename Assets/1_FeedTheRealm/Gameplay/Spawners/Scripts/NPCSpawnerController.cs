using System;
using System.Linq;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class NPCSpawnerController : SpawnerController, IPersistent, IEditable
{
    [SerializeField]
    private MakerInputReader inputReader;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    private UIDocument editorMenu;
    private Slider radiusSlider;
    private DropdownField npcDropdown;
    private Button closeButton;
    private NPCSpawnerData _npcSpawnData;

    public NPCSpawnerData NPCSpawnData
    {
        get { return _npcSpawnData; }
        set
        {
            _npcSpawnData = value;
            transform.position = _npcSpawnData.Position;
            transform.localScale = new Vector3(
                _npcSpawnData.Radius,
                transform.localScale.y,
                _npcSpawnData.Radius
            );
        }
    }

    void OnEnable()
    {
        NPCSpawnData = new NPCSpawnerData(transform.position, transform.localScale.x);
        editorMenu = GetComponent<UIDocument>();
        var root = editorMenu.rootVisualElement;

        radiusSlider = root.Q<Slider>("SpawnerRadius");
        npcDropdown = root.Q<DropdownField>("NPCDropdown");
        closeButton = root.Q<Button>("Close");

        if (creatorObjectLibrary == null)
        {
            Debug.LogError("NPCSpawnerController: CreatorObjectLibrarySO reference is missing.");
            return;
        }

        radiusSlider.value = NPCSpawnData.Radius;

        radiusSlider.RegisterValueChangedCallback(e =>
        {
            NPCSpawnData.Radius = e.newValue;
            transform.localScale = new Vector3(e.newValue, transform.localScale.y, e.newValue);
        });
        closeButton.clicked += CloseMenu;

        editorMenu.rootVisualElement.style.display = DisplayStyle.None;
    }

    void OnDisable()
    {
        closeButton.clicked -= CloseMenu;
    }

    private void RenderMenu(Action CloseEditorCallback)
    {
        inputReader.ToggleInput(false);
        radiusSlider.value = NPCSpawnData.Radius;

        if (npcDropdown != null && creatorObjectLibrary != null)
        {
            var npcs = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.NPC)
                .Cast<GenericNPC>()
                .ToList();

            npcDropdown.choices = npcs.Select(npc => npc.DisplayName).ToList();

            if (!string.IsNullOrEmpty(NPCSpawnData.npcId))
            {
                var selectedNPC = npcs.FirstOrDefault(npc => npc.ObjectId == NPCSpawnData.npcId);
                if (selectedNPC != null)
                {
                    npcDropdown.value = selectedNPC.DisplayName;
                }
            }

            npcDropdown.RegisterValueChangedCallback(e =>
            {
                var selectedNPC = npcs.FirstOrDefault(npc => npc.DisplayName == e.newValue);
                if (selectedNPC != null)
                {
                    NPCSpawnData.npcId = selectedNPC.ObjectId;
                    Debug.Log(
                        $"NPCSpawnerController: Selected NPC '{selectedNPC.DisplayName}' (ID: {selectedNPC.ObjectId})"
                    );
                }
            });
        }

        editorMenu.rootVisualElement.style.display = DisplayStyle.Flex;

        closeButton.clicked += () =>
        {
            CloseMenu();
            CloseEditorCallback?.Invoke();
        };
    }

    private void CloseMenu()
    {
        Debug.Log("NPCSpawnerController: Closing editor menu");
        inputReader.ToggleInput(true);
        editorMenu.rootVisualElement.style.display = DisplayStyle.None;
    }

    public override void SaveData(ref WorldData worldData)
    {
        if (!gameObject.activeSelf)
            return;

        _npcSpawnData.Position = transform.position;
        worldData.npcSpawnAreas.Add(NPCSpawnData);
    }

    public void OnObjectSelected(Action CloseEditorCallback) => RenderMenu(CloseEditorCallback);

    public void OnObjectDeselected()
    {
        CloseMenu();
    }
}
