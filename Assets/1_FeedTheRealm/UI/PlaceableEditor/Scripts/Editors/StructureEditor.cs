using System;
using System.Linq;
using Enums;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Inputs;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.Gameplay.WorldObjects;
using FTR.UI;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.PlaceableEditor
{
    [RequireComponent(typeof(UIDocument))]
    public class StructureEditor : MenuController, IEditable
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [Header("Scroll Sensitivity")]
        [SerializeField]
        private float positionScrollSensitivity = 1f;

        [SerializeField]
        private float rotationScrollSensitivity = 10f;

        [SerializeField]
        private float scaleScrollSensitivity = 1f;

        [Header("Colliders Visualization")]
        [SerializeField]
        private GameObject cubeCollider;

        [SerializeField]
        private GameObject slopeCollider;

        private GameObject cubeColliderInstance;
        private GameObject slopeColliderInstance;

        // UI Elements
        private Label titleLabel;
        private TabView tabView;
        private Vector3Field positionField;
        private Vector3Field rotationField;
        private Vector3Field scaleField;
        private Vector3Field colliderCenterField;
        private Vector3Field colliderSizeField;
        private Vector3Field colliderRotationField;
        private Toggle shopToggle;
        private DropdownField shopDropdown;
        private Button closeButton;
        private Toggle collidersToggle;
        private FloatField focusedAxisField;
        private Vector3Field focusedVectorField;
        private DropdownField colliderTypeDropdown;

        // Target being edited
        private StructureObject target;

        // Collider visuals
        private GameObject ActiveColliderVisual =>
            target.data.colliderType == ColliderType.Cube
                ? cubeColliderInstance
                : slopeColliderInstance;
        private bool isColliderMode;

        // Gizmos
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
            colliderTypeDropdown = root.Q<DropdownField>("ColliderType");
            collidersToggle = root.Q<Toggle>("CollidersToggle");
            colliderTypeDropdown.choices = Enum.GetNames(typeof(ColliderType)).ToList();
            colliderTypeDropdown.RegisterValueChangedCallback(OnColliderTypeChanged);
            tabView = root.Q<TabView>("TabView");

            colliderCenterField = root.Q<Vector3Field>("ColliderCenter");
            colliderSizeField = root.Q<Vector3Field>("ColliderSize");
            colliderRotationField = root.Q<Vector3Field>("ColliderRotation");
            colliderRotationField.RegisterValueChangedCallback(OnColliderRotationChanged);
            colliderCenterField.RegisterValueChangedCallback(OnColliderCenterChanged);
            colliderSizeField.RegisterValueChangedCallback(OnColliderSizeChanged);
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
            RegisterAxisHandlers(colliderCenterField);
            RegisterAxisHandlers(colliderSizeField);
            RegisterAxisHandlers(colliderRotationField);

            root.Q<Button>("ResetColliders").clicked += OnResetColliders;

            tabView.activeTabChanged += (previousTab, newTab) =>
            {
                isColliderMode = newTab.name == "Colliders";
                if (isColliderMode)
                {
                    SyncColliderVisual();
                    ReinitializeGizmosForCollider();
                    ActivateGizmo(positionGizmo);
                }
                else
                {
                    cubeColliderInstance?.SetActive(false);
                    slopeColliderInstance?.SetActive(false);
                    ReinitializeGizmosForTarget();
                    ActivateGizmo(positionGizmo);
                }
            };

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
            colliderRotationField.SetValueWithoutNotify(target.data.colliderRotation);

            colliderTypeDropdown.SetValueWithoutNotify(
                target.data.colliderType == ColliderType.Cube ? "Cube" : "Slope"
            );

            SetupShopControls();
            SetupGizmos();
            SetupColliderVisuals();
            Debug.Log($"[StructureEditor] colliderType at edit time: {target.data.colliderType}");
            SyncColliderVisual();
            SubscribeShortcuts();

            var boxCollider = target.GetComponent<BoxCollider>();
            var initialCenter =
                target.data.colliderCenter != Vector3.zero
                    ? target.data.colliderCenter
                    : boxCollider.center;
            var initialSize =
                target.data.colliderSize != Vector3.zero
                    ? target.data.colliderSize
                    : boxCollider.size;
            colliderCenterField.SetValueWithoutNotify(initialCenter);
            colliderSizeField.SetValueWithoutNotify(initialSize);
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
            if (isColliderMode)
            {
                Vector3 localPosition = target.transform.InverseTransformPoint(newPosition);
                target.data.colliderCenter = localPosition;
                colliderCenterField.SetValueWithoutNotify(localPosition);
                cubeColliderInstance.transform.localPosition = localPosition;
                slopeColliderInstance.transform.localPosition = localPosition;
            }
            else
            {
                positionField.SetValueWithoutNotify(newPosition);
            }
        }

        private void OnGizmoScaled(Vector3 newScale)
        {
            if (isColliderMode)
            {
                target.data.colliderSize = newScale;
                colliderSizeField.SetValueWithoutNotify(newScale);
                slopeColliderInstance.transform.localScale = newScale;
                cubeColliderInstance.transform.localScale = newScale;
            }
            else
            {
                scaleField.SetValueWithoutNotify(newScale);
            }
        }

        private void OnGizmoRotated(Vector3 eulerAngles)
        {
            if (isColliderMode)
            {
                target.data.colliderRotation = eulerAngles;
                colliderRotationField.SetValueWithoutNotify(eulerAngles);
                slopeColliderInstance.transform.localRotation = Quaternion.Euler(eulerAngles);
                cubeColliderInstance.transform.localRotation = Quaternion.Euler(eulerAngles);
            }
            else
            {
                rotationField.SetValueWithoutNotify(eulerAngles);
            }
        }

        private void ReinitializeGizmosForCollider()
        {
            var t = ActiveColliderVisual.transform;
            if (positionGizmo != null)
                positionGizmo.Initialize(t, Camera.main);
            if (scaleGizmo != null)
                scaleGizmo.Initialize(t, Camera.main);
            if (rotationGizmo != null)
                rotationGizmo.Initialize(t, Camera.main);
        }

        private void ReinitializeGizmosForTarget()
        {
            if (positionGizmo != null)
                positionGizmo.Initialize(target.transform, Camera.main);
            if (scaleGizmo != null)
                scaleGizmo.Initialize(target.transform, Camera.main);
            if (rotationGizmo != null)
                rotationGizmo.Initialize(target.transform, Camera.main);
        }

        // ---- Collider Visualization ----

        private void SetupColliderVisuals()
        {
            if (cubeColliderInstance != null)
            {
                Destroy(cubeColliderInstance);
                cubeColliderInstance = null;
            }
            if (slopeColliderInstance != null)
            {
                Destroy(slopeColliderInstance);
                slopeColliderInstance = null;
            }

            var boxCollider = target.GetComponent<BoxCollider>();
            var initialCenter =
                target.data.colliderCenter != Vector3.zero
                    ? target.data.colliderCenter
                    : boxCollider.center;
            var initialSize =
                target.data.colliderSize != Vector3.zero
                    ? target.data.colliderSize
                    : boxCollider.size;

            cubeColliderInstance = Instantiate(cubeCollider, target.transform);
            cubeColliderInstance.transform.localPosition = initialCenter;
            cubeColliderInstance.transform.localRotation = Quaternion.Euler(
                target.data.colliderRotation
            );
            cubeColliderInstance.transform.localScale = initialSize;
            cubeColliderInstance.SetActive(false);

            slopeColliderInstance = Instantiate(slopeCollider, target.transform);
            slopeColliderInstance.transform.localPosition = initialCenter;
            slopeColliderInstance.transform.localRotation = Quaternion.Euler(
                target.data.colliderRotation
            );
            slopeColliderInstance.transform.localScale = initialSize;
            slopeColliderInstance.SetActive(false);
        }

        private void SyncColliderVisual()
        {
            if (cubeColliderInstance == null || slopeColliderInstance == null)
                return;

            if (!isColliderMode)
            {
                cubeColliderInstance.SetActive(false);
                slopeColliderInstance.SetActive(false);
                return;
            }

            bool isCube = target.data.colliderType == ColliderType.Cube;
            cubeColliderInstance.SetActive(isCube);
            slopeColliderInstance.SetActive(!isCube);
        }

        private void SetCubeColliderActive(bool active) => cubeColliderInstance?.SetActive(active);

        // ---- Shortcuts ----

        private void SubscribeShortcuts()
        {
            inputReader.MoveShortcutEvent += OnMoveShortcut;
            inputReader.ScaleShortcutEvent += OnScaleShortcut;
            inputReader.RotateShortcutEvent += OnRotateShortcut;
            inputReader.HideShortcutEvent += OnHideShortcut;
            inputReader.ColliderShortcutEvent += OnColliderShortcut;
        }

        private void UnsubscribeShortcuts()
        {
            inputReader.MoveShortcutEvent -= OnMoveShortcut;
            inputReader.ScaleShortcutEvent -= OnScaleShortcut;
            inputReader.RotateShortcutEvent -= OnRotateShortcut;
            inputReader.HideShortcutEvent -= OnHideShortcut;
            inputReader.ColliderShortcutEvent -= OnColliderShortcut;
        }

        private void OnColliderShortcut()
        {
            var colliderTab = tabView.Q<Tab>("Colliders");
            var transformTab = tabView.Q<Tab>("Transform");
            tabView.activeTab = isColliderMode ? transformTab : colliderTab;
        }

        private void OnMoveShortcut()
        {
            if (!isColliderMode)
                tabView.activeTab = tabView.Q<Tab>("Transform");
            ActivateGizmo(positionGizmo);
        }

        private void OnScaleShortcut()
        {
            if (!isColliderMode)
                tabView.activeTab = tabView.Q<Tab>("Transform");
            ActivateGizmo(scaleGizmo);
        }

        private void OnRotateShortcut()
        {
            if (!isColliderMode)
                tabView.activeTab = tabView.Q<Tab>("Transform");
            ActivateGizmo(rotationGizmo);
        }

        private void OnHideShortcut() => ActivateGizmo(null);

        // ---- Collider Controls ----

        private void OnColliderCenterChanged(ChangeEvent<Vector3> evt)
        {
            target.data.colliderCenter = evt.newValue;
            cubeColliderInstance.transform.localPosition = evt.newValue;
            slopeColliderInstance.transform.localPosition = evt.newValue;
        }

        private void OnColliderSizeChanged(ChangeEvent<Vector3> evt)
        {
            target.data.colliderSize = evt.newValue;
            cubeColliderInstance.transform.localScale = evt.newValue;
            slopeColliderInstance.transform.localScale = evt.newValue;
        }

        private void OnColliderRotationChanged(ChangeEvent<Vector3> evt)
        {
            target.data.colliderRotation = evt.newValue;
            cubeColliderInstance.transform.localRotation = Quaternion.Euler(evt.newValue);
            slopeColliderInstance.transform.localRotation = Quaternion.Euler(evt.newValue);
        }

        private void OnResetColliders()
        {
            var boxCollider = target.GetComponent<BoxCollider>();
            target.data.colliderCenter = boxCollider.center;
            target.data.colliderSize = boxCollider.size;
            target.data.colliderRotation = Vector3.zero;

            colliderCenterField.SetValueWithoutNotify(boxCollider.center);
            colliderSizeField.SetValueWithoutNotify(boxCollider.size);
            colliderRotationField.SetValueWithoutNotify(Vector3.zero);

            cubeColliderInstance.transform.localPosition = boxCollider.center;
            cubeColliderInstance.transform.localScale = boxCollider.size;
            cubeColliderInstance.transform.localRotation = Quaternion.identity;
            slopeColliderInstance.transform.localPosition = boxCollider.center;
            slopeColliderInstance.transform.localScale = boxCollider.size;
            slopeColliderInstance.transform.localRotation = Quaternion.identity;
        }

        private void OnColliderTypeChanged(ChangeEvent<string> evt)
        {
            target.data.colliderType = Enum.Parse<ColliderType>(evt.newValue);
            SyncColliderVisual();
            if (isColliderMode)
                ReinitializeGizmosForCollider();
        }

        private void OnCollidersToggleChanged(ChangeEvent<bool> evt)
        {
            target.data.hasColliders = evt.newValue;
        }

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

            SetCubeColliderActive(false);
            activeGizmo = null;
            focusedAxisField = null;
            focusedVectorField = null;
            Destroy(cubeColliderInstance);
            Destroy(slopeColliderInstance);
            cubeColliderInstance = null;
            slopeColliderInstance = null;
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
