using Models;
using UnityEngine;
using UnityEngine.UIElements;

public class StructureController : MonoBehaviour, IPersistent, ISelectable
{
    [SerializeField]
    private UIDocument structureUI;

    [SerializeField]
    private MakerInputReader inputReader;

    [SerializeField]
    private float positionScrollSensitivity = 1f;

    [SerializeField]
    private float rotationScrollSensitivity = 10f;

    [SerializeField]
    private float scaleScrollSensitivity = 1f;

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
    private FloatField focusedAxisField;
    private Vector3Field focusedVectorField;

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

        RegisterAxisFocusListeners(positionField);
        RegisterAxisFocusListeners(rotationField);
        RegisterAxisFocusListeners(scaleField);

        inputReader.ScrollEvent += OnScroll;
        closeButton.clicked += OnClose;
    }

    public void OnClose()
    {
        inputReader.ToggleInput(true);
        structureUI.rootVisualElement.style.display = DisplayStyle.None;
        closeButton.clicked -= OnClose;
        inputReader.ScrollEvent -= OnScroll;
        focusedAxisField = null;
    }

    private void RegisterAxisFocusListeners(Vector3Field field)
    {
        var vectorInput = field.Q(className: "unity-vector3-field__input");
        if (vectorInput == null)
            return;

        var xInput = vectorInput.Q<FloatField>(name: "unity-x-input");
        var yInput = vectorInput.Q<FloatField>(name: "unity-y-input");
        var zInput = vectorInput.Q<FloatField>(name: "unity-z-input");

        xInput.RegisterCallback<FocusInEvent>(_ =>
        {
            focusedAxisField = xInput;
            focusedVectorField = field;
        });
        xInput.RegisterCallback<FocusOutEvent>(_ =>
        {
            focusedAxisField = null;
            focusedVectorField = null;
        });
        yInput.RegisterCallback<FocusInEvent>(_ =>
        {
            focusedAxisField = yInput;
            focusedVectorField = field;
        });
        yInput.RegisterCallback<FocusOutEvent>(_ =>
        {
            focusedAxisField = null;
            focusedVectorField = null;
        });
        zInput.RegisterCallback<FocusInEvent>(_ =>
        {
            focusedAxisField = zInput;
            focusedVectorField = field;
        });
        zInput.RegisterCallback<FocusOutEvent>(_ =>
        {
            focusedAxisField = null;
            focusedVectorField = null;
        });
    }

    private void OnScroll(Vector2 scrollValue)
    {
        if (focusedAxisField == null || focusedVectorField == null)
            return;

        float sensitivity = GetSensitivityForField(focusedVectorField);
        float scrollDelta = scrollValue.y > 0 ? 1f * sensitivity : -1f * sensitivity;
        focusedAxisField.value += scrollDelta;
    }

    private float GetSensitivityForField(Vector3Field field)
    {
        return field switch
        {
            _ when field == positionField => positionScrollSensitivity,
            _ when field == rotationField => rotationScrollSensitivity,
            _ when field == scaleField => scaleScrollSensitivity,
            _ => 1f,
        };
    }
}
