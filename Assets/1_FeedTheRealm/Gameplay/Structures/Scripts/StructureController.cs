using Models;
using UnityEngine;
using UnityEngine.UIElements;

public class StructureController : MonoBehaviour, IPersistent, ISelectable
{
    [SerializeField]
    private UIDocument structureUI;

    public string structureName;
    public string id;
    public Vector3 size = Vector3.one;
    public Vector3 rotation;
    public Vector3 offset;
    public string objectUrl;

    // UI Elements
    private Label titleLabel;
    private Vector3Field positionField;
    private Vector3Field rotationField;
    private Vector3Field scaleField;
    private Button closeButton;

    void OnEnable()
    {
        if (structureUI == null)
            return;
        structureUI.enabled = false;
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
            Structure.name,
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
        if (structureUI == null)
            return;

        structureUI.enabled = true;

        if (titleLabel != null)
            titleLabel.text = name;

        if (positionField != null)
            positionField.value = transform.position;
        if (rotationField != null)
            rotationField.value = transform.eulerAngles;
        if (scaleField != null)
            scaleField.value = transform.localScale;

        if (positionField != null)
            positionField.RegisterValueChangedCallback(evt => transform.position = evt.newValue);
        if (rotationField != null)
            rotationField.RegisterValueChangedCallback(evt => transform.eulerAngles = evt.newValue);
        if (scaleField != null)
            scaleField.RegisterValueChangedCallback(evt => transform.localScale = evt.newValue);

        if (closeButton != null)
            closeButton.clicked += OnObjectDeselected;
    }

    public void OnObjectDeselected()
    {
        if (structureUI == null)
            return;

        structureUI.enabled = false;
        if (closeButton != null)
            closeButton.clicked -= OnObjectDeselected;
    }
}
