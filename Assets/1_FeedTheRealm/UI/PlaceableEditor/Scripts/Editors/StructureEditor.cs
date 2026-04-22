using System.Linq;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Inputs;
using FeedTheRealm.Gameplay.Library;
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
        private Toggle collidersToggle;

        private FloatField focusedAxisField;
        private Vector3Field focusedVectorField;
        private StructureObject target;
        private PositionGizmo positionGizmo;
        private ScaleGizmo scaleGizmo;
        private BaseGizmo activeGizmo;
        private RotationGizmo rotationGizmo;

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
            collidersToggle = root.Q<Toggle>("CollidersToggle");
            collidersToggle.RegisterValueChangedCallback(OnCollidersToggleChanged);

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
            collidersToggle.SetValueWithoutNotify(target.data.hasColliders);
            scaleField.SetValueWithoutNotify(target.transform.localScale);

            SetupShopControls();
            SetupGizmos();
            SubscribeShortcuts();

            inputReader.ScrollEvent += OnScroll;
        }

        // ---- Gizmo Setup ----

        private void SetupGizmos()
        {
            positionGizmo = GetComponentInChildren<PositionGizmo>(includeInactive: true);
            scaleGizmo = GetComponentInChildren<ScaleGizmo>(includeInactive: true);
            rotationGizmo = GetComponentInChildren<RotationGizmo>(includeInactive: true);

            if (positionGizmo != null)
            {
                positionGizmo.Initialize(target.transform, Camera.main);
                positionGizmo.OnPositionChanged += OnGizmoMoved;
            }
            if (scaleGizmo != null)
            {
                scaleGizmo.Initialize(target.transform, Camera.main);
                scaleGizmo.OnScaleChanged += OnGizmoScaled;
            }
            if (rotationGizmo != null)
            {
                rotationGizmo.Initialize(target.transform, Camera.main);
                rotationGizmo.OnRotationChanged += OnGizmoRotated;
            }

            ActivateGizmo(positionGizmo);
        }

        private void ActivateGizmo(BaseGizmo gizmo)
        {
            // disable all gizmos first
            if (positionGizmo != null)
                positionGizmo.gameObject.SetActive(false);
            if (scaleGizmo != null)
                scaleGizmo.gameObject.SetActive(false);
            if (rotationGizmo != null)
                rotationGizmo.gameObject.SetActive(false);

            activeGizmo = gizmo;

            if (activeGizmo != null)
                activeGizmo.gameObject.SetActive(true);
        }

        private void OnGizmoMoved(Vector3 newPosition)
        {
            positionField.SetValueWithoutNotify(newPosition);
        }

        private void OnGizmoScaled(Vector3 newScale)
        {
            scaleField.SetValueWithoutNotify(newScale);
        }

        private void OnGizmoRotated(Vector3 eulerAngles)
        {
            rotationField.SetValueWithoutNotify(eulerAngles);
        }

        // ---- Shortcuts ----

        private void SubscribeShortcuts()
        {
            inputReader.MoveShortcutEvent += OnMoveShortcut;
            inputReader.ScaleShortcutEvent += OnScaleShortcut;
            inputReader.RotateShortcutEvent += OnRotateShortcut;
            inputReader.HideShortcutEvent += OnHideShortcut;
        }

        private void UnsubscribeShortcuts()
        {
            inputReader.MoveShortcutEvent -= OnMoveShortcut;
            inputReader.ScaleShortcutEvent -= OnScaleShortcut;
            inputReader.RotateShortcutEvent -= OnRotateShortcut;
            inputReader.HideShortcutEvent -= OnHideShortcut;
        }

        private void OnMoveShortcut() => ActivateGizmo(positionGizmo);

        private void OnScaleShortcut() => ActivateGizmo(scaleGizmo);

        private void OnHideShortcut() => ActivateGizmo(null);

        private void OnRotateShortcut() => ActivateGizmo(rotationGizmo);

        // ---- Shop Controls ----

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

        private void OnCollidersToggleChanged(ChangeEvent<bool> evt)
        {
            target.data.hasColliders = evt.newValue;
        }

        // ---- Close ----

        public override void CloseMenu()
        {
            inputReader.ScrollEvent -= OnScroll;
            UnsubscribeShortcuts();

            if (positionGizmo != null)
            {
                positionGizmo.OnPositionChanged -= OnGizmoMoved;
                positionGizmo.gameObject.SetActive(false);
            }

            if (scaleGizmo != null)
            {
                scaleGizmo.OnScaleChanged -= OnGizmoScaled;
                scaleGizmo.gameObject.SetActive(false);
            }

            activeGizmo = null;
            focusedAxisField = null;
            focusedVectorField = null;
            base.CloseMenu();
        }

        // ---- Scroll ----

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
