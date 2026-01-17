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

    // Data
    public StructureData structureData;

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
        var root = structureUI.rootVisualElement;
        root.style.display = DisplayStyle.None;

        titleLabel = root.Q<Label>("StructureName");
        positionField = root.Q<Vector3Field>("Position");
        rotationField = root.Q<Vector3Field>("Rotation");
        scaleField = root.Q<Vector3Field>("Scale");
        closeButton = root.Q<Button>("Close");
    }

    public GameObject Structure =>
        transform.childCount > 0 ? transform.GetChild(0).gameObject : null;

    public void SaveData(ref WorldData worldData)
    {
        if (!gameObject.activeSelf)
            return;

        worldData.objectPlacementData.Add(
            new StructureData(
                structureData.structureName,
                name,
                transform.localScale,
                transform.eulerAngles,
                Vector3.zero,
                transform.position
            )
        );
    }

    public void OnObjectSelected() => RenderMenu();

    public void RenderMenu()
    {
        structureUI.rootVisualElement.style.display = DisplayStyle.Flex;

        titleLabel.text = name;

        positionField.value = transform.position;
        rotationField.value = transform.eulerAngles;
        scaleField.value = transform.localScale;

        positionField.RegisterValueChangedCallback(e => transform.position = e.newValue);
        rotationField.RegisterValueChangedCallback(e => transform.eulerAngles = e.newValue);
        scaleField.RegisterValueChangedCallback(e => transform.localScale = e.newValue);

        RegisterAxisHandlers(positionField);
        RegisterAxisHandlers(rotationField);
        RegisterAxisHandlers(scaleField);

        inputReader.ScrollEvent += OnScroll;
        closeButton.clicked += OnClose;
    }

    public void OnClose()
    {
        structureUI.rootVisualElement.style.display = DisplayStyle.None;

        closeButton.clicked -= OnClose;
        inputReader.ScrollEvent -= OnScroll;

        focusedAxisField = null;
        focusedVectorField = null;
    }

    // -------------------- Private Methods --------------------

    private void RegisterAxisHandlers(Vector3Field field)
    {
        var vectorInput = field.Q(className: "unity-vector3-field__input");
        RegisterAxis(vectorInput.Q<FloatField>("unity-x-input"), field);
        RegisterAxis(vectorInput.Q<FloatField>("unity-y-input"), field);
        RegisterAxis(vectorInput.Q<FloatField>("unity-z-input"), field);
    }

    private void RegisterAxis(FloatField axis, Vector3Field parent)
    {
        axis.RegisterCallback<FocusInEvent>(_ =>
        {
            focusedAxisField = axis;
            focusedVectorField = parent;
        });
        axis.RegisterCallback<FocusOutEvent>(_ =>
        {
            focusedAxisField = null;
            focusedVectorField = null;
        });
    }

    private void OnScroll(Vector2 scrollValue)
    {
        float sensitivity = GetSensitivityForField(focusedVectorField);
        float delta = scrollValue.y > 0 ? sensitivity : -sensitivity;
        focusedAxisField.value += delta;
    }

    private float GetSensitivityForField(Vector3Field field) =>
        field == positionField ? positionScrollSensitivity
        : field == rotationField ? rotationScrollSensitivity
        : field == scaleField ? scaleScrollSensitivity
        : 1f;
}
