using System.Linq;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Inputs;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.Gameplay.WorldEditor;
using FeedTheRealm.Gameplay.WorldObjects;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.PlaceableEditor
{
    [RequireComponent(typeof(UIDocument))]
    public class StructureEditor : MenuController, IEditable
    {
        [Inject]
        private InputReader inputReader;

        [Inject]
        private CreatablesManager creatablesManager;

        [SerializeField]
        private float positionScrollSensitivity = 1f;

        [SerializeField]
        private float rotationScrollSensitivity = 10f;

        [SerializeField]
        private float scaleScrollSensitivity = 1f;

        private Label titleLabel;
        private Vector3Field positionField;
        private Vector3Field rotationField;
        private Vector3Field scaleField;
        private Toggle shopToggle;
        private DropdownField shopDropdown;
        private Button closeButton;

        private FloatField focusedAxisField;
        private Vector3Field focusedVectorField;
        private StructureObject target;
        private TransformGizmo gizmo;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            titleLabel = root.Q<Label>("StructureName");
            positionField = root.Q<Vector3Field>("Position");
            rotationField = root.Q<Vector3Field>("Rotation");
            scaleField = root.Q<Vector3Field>("Scale");
            shopToggle = root.Q<Toggle>("ShopToggle");
            shopDropdown = root.Q<DropdownField>("ShopDropdown");
            closeButton = root.Q<Button>("Close");

            positionField.RegisterValueChangedCallback(e => target.transform.position = e.newValue);
            rotationField.RegisterValueChangedCallback(e =>
                target.transform.localEulerAngles = e.newValue
            );
            scaleField.RegisterValueChangedCallback(e => target.transform.localScale = e.newValue);

            shopToggle.RegisterValueChangedCallback(OnShopToggleChanged);
            shopDropdown.RegisterValueChangedCallback(OnShopSelected);

            RegisterAxisHandlers(positionField);
            RegisterAxisHandlers(rotationField);
            RegisterAxisHandlers(scaleField);

            closeButton.clicked += CloseMenu;
        }

        public void Edit(GameObject placeable)
        {
            target = placeable.GetComponent<StructureObject>();
            if (target == null)
            {
                Debug.LogError(
                    $"StructureEditor: {placeable.name} has no StructureObject component."
                );
                Destroy(gameObject);
                return;
            }

            titleLabel.text = target.name;
            positionField.SetValueWithoutNotify(target.transform.position);
            rotationField.SetValueWithoutNotify(target.transform.localEulerAngles);
            scaleField.SetValueWithoutNotify(target.transform.localScale);

            SetupShopControls();

            gizmo = target.GetComponentInChildren<TransformGizmo>(includeInactive: true);
            if (gizmo != null)
            {
                gizmo.Initialize(target.transform, Camera.main);
                gizmo.OnPositionChanged += OnGizmoMoved;
                gizmo.gameObject.SetActive(true);
            }

            inputReader.ScrollEvent += OnScroll;
        }

        private void OnGizmoMoved(Vector3 newPosition)
        {
            positionField.SetValueWithoutNotify(newPosition);
        }

        private void SetupShopControls()
        {
            var shops = creatablesManager.GetAll<Shop>();

            if (shops.Count == 0)
            {
                shopToggle.SetEnabled(false);
                shopToggle.SetValueWithoutNotify(false);
                target.data.isShop = false;
                target.data.shopId = null;
                shopDropdown.style.display = DisplayStyle.None;
                return;
            }

            shopToggle.SetEnabled(true);
            shopDropdown.choices = shops.Select(s => s.data.shopName).ToList();

            bool hasPersistedShop = target.data.isShop && !string.IsNullOrEmpty(target.data.shopId);
            var currentShop = hasPersistedShop
                ? shops.FirstOrDefault(s => s.Id == target.data.shopId)
                : null;
            if (hasPersistedShop && currentShop == null)
            {
                target.data.isShop = false;
                target.data.shopId = null;
            }
            bool hasShop = currentShop != null;
            shopToggle.SetValueWithoutNotify(hasShop);
            shopDropdown.style.display = hasShop ? DisplayStyle.Flex : DisplayStyle.None;
            if (currentShop != null)
                shopDropdown.SetValueWithoutNotify(currentShop.data.shopName);
        }

        private void OnShopToggleChanged(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                shopDropdown.style.display = DisplayStyle.Flex;

                // auto select first shop if none selected
                var shops = creatablesManager.GetAll<Shop>();
                if (shops.Count > 0 && string.IsNullOrEmpty(target.data.shopId))
                {
                    target.data.isShop = true;
                    target.data.shopId = shops[0].Id;
                    shopDropdown.SetValueWithoutNotify(shops[0].data.shopName);
                }
            }
            else
            {
                shopDropdown.style.display = DisplayStyle.None;
                target.data.isShop = false;
                target.data.shopId = null;
            }
        }

        private void OnShopSelected(ChangeEvent<string> evt)
        {
            var selected = creatablesManager
                .GetAll<Shop>()
                .FirstOrDefault(s => s.data.shopName == evt.newValue);

            if (selected == null)
                return;

            target.data.isShop = true;
            target.data.shopId = selected.Id;
        }

        public override void CloseMenu()
        {
            inputReader.ScrollEvent -= OnScroll;

            if (gizmo != null)
                gizmo.OnPositionChanged -= OnGizmoMoved;

            focusedAxisField = null;
            focusedVectorField = null;
            gizmo.gameObject.SetActive(false);
            base.CloseMenu();
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
            var input = field.Q(className: "unity-vector3-field__input");
            RegisterAxis(input.Q<FloatField>("unity-x-input"), field);
            RegisterAxis(input.Q<FloatField>("unity-y-input"), field);
            RegisterAxis(input.Q<FloatField>("unity-z-input"), field);
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
}
