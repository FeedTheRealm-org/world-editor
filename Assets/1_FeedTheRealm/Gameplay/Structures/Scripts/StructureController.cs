using System;
using System.Linq;
using FeedTheRealm.Gameplay.Inputs;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

public class StructureController : MonoBehaviour, IPersistent, IEditable
{
    [SerializeField]
    private UIDocument structureUI;

    [SerializeField]
    private InputReader inputReader;

    [SerializeField]
    private float positionScrollSensitivity = 1f;

    [SerializeField]
    private float rotationScrollSensitivity = 10f;

    [SerializeField]
    private float scaleScrollSensitivity = 1f;

    [SerializeField]
    private ShopManagerSO shopManager;

    public bool isShop = false;
    public string shopId;

    // Data
    public GameObject Structure =>
        transform.childCount > 0 ? transform.GetChild(0).gameObject : null;

    // UI Elements
    private Label titleLabel;
    private Vector3Field positionField;
    private Vector3Field rotationField;
    private Vector3Field scaleField;
    private Button closeButton;
    private Toggle shopToggle;
    private DropdownField shopDropdown;

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
        shopToggle = root.Q<Toggle>("ShopToggle");
        shopDropdown = root.Q<DropdownField>("ShopDropdown");
    }

    public void SaveData(ref WorldData worldData)
    {
        if (!gameObject.activeSelf)
            return;

        if (Structure == null)
            return;

        BoxCollider collider = Structure?.GetComponent<BoxCollider>();
        Vector3 colliderSize = collider != null ? collider.size : Vector3.zero;
        Vector3 colliderCenter = collider != null ? collider.center : Vector3.zero;

        StructureData structureData = new(
            Structure.name,
            name,
            transform.localScale,
            transform.localEulerAngles,
            Vector3.zero,
            transform.position,
            isShop,
            colliderSize,
            colliderCenter,
            isShop ? shopId : null
        );

        worldData.objectPlacementData.Add(structureData);
    }

    public void OnObjectSelected(Action CloseEditorCallback) => RenderMenu(CloseEditorCallback);

    public void OnObjectDeselected()
    {
        CloseMenu();
    }

    // -------------------- Private Methods --------------------

    private void RenderMenu(Action CloseEditorCallback)
    {
        structureUI.rootVisualElement.style.display = DisplayStyle.Flex;

        titleLabel.text = name;

        positionField.value = transform.position;
        rotationField.value = transform.localEulerAngles;
        scaleField.value = transform.localScale;
        shopToggle.value = isShop;

        shopToggle.RegisterValueChangedCallback(e =>
        {
            isShop = e.newValue;
            UpdateShopDropdownVisibility(e.newValue);
        });
        positionField.RegisterValueChangedCallback(e => transform.position = e.newValue);
        rotationField.RegisterValueChangedCallback(e => transform.localEulerAngles = e.newValue);
        scaleField.RegisterValueChangedCallback(e => transform.localScale = e.newValue);

        PopulateShopDropdown();
        UpdateShopDropdownVisibility(isShop);
        shopDropdown.RegisterValueChangedCallback(e =>
        {
            var shop = shopManager?.GetShops().Find(s => s.displayName == e.newValue);
            shopId = shop?.id;
        });

        RegisterAxisHandlers(positionField);
        RegisterAxisHandlers(rotationField);
        RegisterAxisHandlers(scaleField);

        inputReader.ScrollEvent += OnScroll;
        closeButton.clicked += () =>
        {
            CloseMenu();
            CloseEditorCallback?.Invoke();
        };
    }

    private void PopulateShopDropdown()
    {
        if (shopManager == null || shopDropdown == null)
            return;

        shopDropdown.choices.Clear();
        foreach (var shop in shopManager.GetShops())
            shopDropdown.choices.Add(shop.displayName);

        if (!string.IsNullOrEmpty(shopId))
        {
            var current = shopManager.GetShops().Find(s => s.id == shopId);
            if (current != null)
            {
                shopDropdown.SetValueWithoutNotify(current.displayName);
            }
            else if (shopDropdown.choices.Count > 0)
            {
                var first = shopManager.GetShops().FirstOrDefault();
                shopId = first?.id;
                shopDropdown.SetValueWithoutNotify(shopDropdown.choices[0]);
            }
        }
        else if (shopDropdown.choices.Count > 0)
        {
            var first = shopManager.GetShops().FirstOrDefault();
            shopId = first?.id;
            shopDropdown.SetValueWithoutNotify(shopDropdown.choices[0]);
        }
    }

    private void UpdateShopDropdownVisibility(bool visible)
    {
        if (shopDropdown == null)
            return;
        shopDropdown.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void CloseMenu()
    {
        structureUI.rootVisualElement.style.display = DisplayStyle.None;
        inputReader.ScrollEvent -= OnScroll;
        focusedAxisField = null;
        focusedVectorField = null;
    }

    private void OnScroll(Vector2 scrollValue)
    {
        if (focusedAxisField == null || focusedVectorField == null)
            return;
        float sensitivity = GetSensitivityForField(focusedVectorField);
        float delta = scrollValue.y > 0 ? sensitivity : -sensitivity;
        focusedAxisField.value += delta;
    }

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

    private float GetSensitivityForField(Vector3Field field) =>
        field == positionField ? positionScrollSensitivity
        : field == rotationField ? rotationScrollSensitivity
        : field == scaleField ? scaleScrollSensitivity
        : 1f;
}
