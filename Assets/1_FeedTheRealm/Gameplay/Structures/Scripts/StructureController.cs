using Models;
using UnityEngine;
using UnityEngine.UIElements;

public class StructureController : MonoBehaviour, IPersistent, ISelectable
{
    [SerializeField]
    private UIDocument structureUI;

    [SerializeField]
    private MakerInputReader inputReader;

    public StructureData structureData;

    // public string structureName;
    // public string id;
    // public Vector3 size = Vector3.one;
    // public Vector3 rotation;
    // public Vector3 offset;
    // public string objectUrl;

    // UI Elements
    private Label titleLabel;
    private Vector3Field positionField;
    private Vector3Field rotationField;
    private Vector3Field scaleField;
    private Button closeButton;

    void OnEnable()
    {
        structureUI.rootVisualElement.style.display = DisplayStyle.None;
        VisualElement root = structureUI.rootVisualElement;
        titleLabel = root.Q<Label>("StructureName");
        positionField = root.Q<Vector3Field>("Position");
        rotationField = root.Q<Vector3Field>("Rotation");
        scaleField = root.Q<Vector3Field>("Scale");
        closeButton = root.Q<Button>("Close");
    }

    public GameObject Structure
    {
        get => transform.childCount > 0 ? transform.GetChild(0).gameObject : null;
    }

    public void SaveData(ref WorldData worldData)
    {
        if (!gameObject.activeSelf)
            return;

        StructureData structureData = new(
            this.structureData.structureName,
            name,
            transform.localScale,
            transform.eulerAngles,
            Vector3.zero,
            transform.position
        );
        worldData.objectPlacementData.Add(structureData);
    }

    public void OnObjectSelected()
    {
        RenderMenu();
    }

    public void RenderMenu()
    {
        inputReader.ToggleInput(false);
        structureUI.rootVisualElement.style.display = DisplayStyle.Flex;
        titleLabel.text = name;
        positionField.value = transform.position;
        rotationField.value = transform.eulerAngles;
        scaleField.value = transform.localScale;
        positionField.RegisterValueChangedCallback(evt => transform.position = evt.newValue);
        rotationField.RegisterValueChangedCallback(evt => transform.eulerAngles = evt.newValue);
        scaleField.RegisterValueChangedCallback(evt => transform.localScale = evt.newValue);
        closeButton.clicked += OnClose;
    }

    public void OnClose()
    {
        inputReader.ToggleInput(true);
        structureUI.rootVisualElement.style.display = DisplayStyle.None;
        closeButton.clicked -= OnClose;
    }
}
