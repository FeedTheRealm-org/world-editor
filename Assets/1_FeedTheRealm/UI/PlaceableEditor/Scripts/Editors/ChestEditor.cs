using System.Linq;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Inputs;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FeedTheRealm.Gameplay.WorldObjects;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.PlaceableEditor
{
    [RequireComponent(typeof(UIDocument))]
    public class ChestEditor : MenuController, IEditable
    {
        [Inject]
        private InputReader inputReader;

        [Inject]
        private CreatablesManager creatablesManager;

        [Inject]
        private PlaceablesLibrary placeablesLibrary;

        [Header("Scroll Sensitivity")]
        [SerializeField]
        private float positionScrollSensitivity = 1f;

        [SerializeField]
        private float rotationScrollSensitivity = 10f;

        [SerializeField]
        private float scaleScrollSensitivity = 1f;

        // UI Elements
        private Label headerLabel;
        private TabView tabView;
        private TextField chestNameField;
        private DropdownField chestLootSelector;
        private DropdownField closedModelSelector;
        private DropdownField openedModelSelector;
        private Button closeButton;
        private Button resetTransformButton;

        // Chest tab
        private Vector3Field positionField;
        private Vector3Field rotationField;
        private Vector3Field scaleField;

        // Closed chest tab
        private Vector3Field closedPositionField;
        private Vector3Field closedRotationField;
        private Vector3Field closedScaleField;

        // Opened chest tab
        private Vector3Field openedPositionField;
        private Vector3Field openedRotationField;
        private Vector3Field openedScaleField;

        private FloatField focusedAxisField;
        private Vector3Field focusedVectorField;

        // Target
        private ChestObject target;
        private string activeTab = "Chest";

        // Gizmos
        private PositionGizmo positionGizmo;
        private ScaleGizmo scaleGizmo;
        private RotationGizmo rotationGizmo;
        private BaseGizmo activeGizmo;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            headerLabel = root.Q<Label>("Header");
            chestNameField = root.Q<TextField>("ChestName");
            chestLootSelector = root.Q<DropdownField>("ChestLootSelector");
            closedModelSelector = root.Q<DropdownField>("ClosedModelSelector");
            openedModelSelector = root.Q<DropdownField>("OpenedModelSelector");
            closeButton = root.Q<Button>("Close");
            resetTransformButton = root.Q<Button>("ResetTransform");
            tabView = root.Q<TabView>("TabView");

            positionField = root.Q<Vector3Field>("Position");
            rotationField = root.Q<Vector3Field>("Rotation");
            scaleField = root.Q<Vector3Field>("Scale");

            closedPositionField = root.Q<Vector3Field>("ClosedPosition");
            closedRotationField = root.Q<Vector3Field>("ClosedRotation");
            closedScaleField = root.Q<Vector3Field>("ClosedScale");

            openedPositionField = root.Q<Vector3Field>("OpendPosition");
            openedRotationField = root.Q<Vector3Field>("OpendRotation");
            openedScaleField = root.Q<Vector3Field>("OpendScale");

            // chest root transform
            positionField.RegisterValueChangedCallback(e => target.transform.position = e.newValue);
            rotationField.RegisterValueChangedCallback(e =>
                target.transform.localEulerAngles = e.newValue
            );
            scaleField.RegisterValueChangedCallback(e => target.transform.localScale = e.newValue);

            // closed model offsets
            closedPositionField.RegisterValueChangedCallback(e =>
            {
                target.data.closedChestModelData.relativePosition = e.newValue;
                if (target.GetClosedModel() != null)
                    target.GetClosedModel().transform.localPosition = e.newValue;
                target.SyncColliderToActiveModel();
            });
            closedRotationField.RegisterValueChangedCallback(e =>
            {
                target.data.closedChestModelData.relativeRotation = e.newValue;
                if (target.GetClosedModel() != null)
                    target.GetClosedModel().transform.localRotation = Quaternion.Euler(e.newValue);
                target.SyncColliderToActiveModel();
            });
            closedScaleField.RegisterValueChangedCallback(e =>
            {
                target.data.closedChestModelData.relativeSize = e.newValue;
                if (target.GetClosedModel() != null)
                    target.GetClosedModel().transform.localScale = e.newValue;
                target.SyncColliderToActiveModel();
            });

            // opened model offsets
            openedPositionField.RegisterValueChangedCallback(e =>
            {
                target.data.opendedChestModelData.relativePosition = e.newValue;
                if (target.GetOpenedModel() != null)
                    target.GetOpenedModel().transform.localPosition = e.newValue;
                target.SyncColliderToActiveModel();
            });
            openedRotationField.RegisterValueChangedCallback(e =>
            {
                target.data.opendedChestModelData.relativeRotation = e.newValue;
                if (target.GetOpenedModel() != null)
                    target.GetOpenedModel().transform.localRotation = Quaternion.Euler(e.newValue);
                target.SyncColliderToActiveModel();
            });
            openedScaleField.RegisterValueChangedCallback(e =>
            {
                target.data.opendedChestModelData.relativeSize = e.newValue;
                if (target.GetOpenedModel() != null)
                    target.GetOpenedModel().transform.localScale = e.newValue;
                target.SyncColliderToActiveModel();
            });

            chestNameField.RegisterValueChangedCallback(e =>
            {
                target.data.name = e.newValue;
                target.gameObject.name = e.newValue;
            });

            chestNameField.RegisterCallback<FocusInEvent>(_ =>
            {
                EnableMovementToggle(false);
                UnsubscribeShortcuts();
            });

            chestNameField.RegisterCallback<FocusOutEvent>(_ =>
            {
                EnableMovementToggle(true);
                SubscribeShortcuts();
            });

            chestLootSelector.RegisterValueChangedCallback(OnLootSelected);
            closedModelSelector.RegisterValueChangedCallback(OnClosedModelSelected);
            openedModelSelector.RegisterValueChangedCallback(OnOpenedModelSelected);

            RegisterAxisHandlers(positionField);
            RegisterAxisHandlers(rotationField);
            RegisterAxisHandlers(scaleField);
            RegisterAxisHandlers(closedPositionField);
            RegisterAxisHandlers(closedRotationField);
            RegisterAxisHandlers(closedScaleField);
            RegisterAxisHandlers(openedPositionField);
            RegisterAxisHandlers(openedRotationField);
            RegisterAxisHandlers(openedScaleField);

            closeButton.clicked += CloseMenu;
            resetTransformButton.clicked += OnResetTransform;

            tabView.activeTabChanged += (previousTab, newTab) =>
            {
                if (target == null)
                    return;
                activeTab = newTab.name;
                Debug.Log($"Tab changed to: {activeTab}");
                SyncModelVisibility();
                ReinitializeGizmosForActiveTab();
            };
        }

        public void Edit(GameObject placeable)
        {
            target = placeable.GetComponent<ChestObject>();
            if (target == null)
            {
                Debug.LogError($"ChestEditor: {placeable.name} has no ChestObject component.");
                Destroy(gameObject);
                return;
            }

            headerLabel.text = target.data.name;
            chestNameField.SetValueWithoutNotify(target.data.name);

            positionField.SetValueWithoutNotify(target.transform.position);
            rotationField.SetValueWithoutNotify(target.transform.localEulerAngles);
            scaleField.SetValueWithoutNotify(target.transform.localScale);

            closedPositionField.SetValueWithoutNotify(
                target.data.closedChestModelData?.relativePosition ?? Vector3.zero
            );
            closedRotationField.SetValueWithoutNotify(
                target.data.closedChestModelData?.relativeRotation ?? Vector3.zero
            );
            closedScaleField.SetValueWithoutNotify(
                target.data.closedChestModelData?.relativeSize ?? Vector3.one
            );

            openedPositionField.SetValueWithoutNotify(
                target.data.opendedChestModelData?.relativePosition ?? Vector3.zero
            );
            openedRotationField.SetValueWithoutNotify(
                target.data.opendedChestModelData?.relativeRotation ?? Vector3.zero
            );
            openedScaleField.SetValueWithoutNotify(
                target.data.opendedChestModelData?.relativeSize ?? Vector3.one
            );

            SetupLootDropdown();
            SetupModelDropdowns();
            SetupGizmos();
            SyncModelVisibility();
            SubscribeShortcuts();

            inputReader.ScrollEvent += OnScroll;
        }

        // ---- Model Visibility ----

        private void SyncModelVisibility()
        {
            bool isOpenTab = activeTab == "OpenedChest";
            target.ToggleChestModels(isOpenTab);
            target.SyncColliderToActiveModel();
        }

        // ---- Dropdowns ----

        private void SetupLootDropdown()
        {
            var lootTables = creatablesManager.GetAll<LootTable>();
            chestLootSelector.choices = lootTables.Select(lt => lt.data.name).ToList();

            if (!string.IsNullOrEmpty(target.data.lootTableId))
            {
                var selected = lootTables.FirstOrDefault(lt =>
                    lt.data.id == target.data.lootTableId
                );
                chestLootSelector.SetValueWithoutNotify(selected?.data.name ?? string.Empty);
            }
        }

        private void SetupModelDropdowns()
        {
            var options = placeablesLibrary.GetPlaceableOptions(
                PlaceableObjectCategories.Structure
            );
            var names = options.Select(o => o.displayName).ToList();

            closedModelSelector.choices = names;
            openedModelSelector.choices = names;

            if (!string.IsNullOrEmpty(target.data.closedChestModelData?.modelId))
            {
                var match = options.FirstOrDefault(o =>
                    o.id == target.data.closedChestModelData.modelId
                );
                closedModelSelector.SetValueWithoutNotify(match.displayName ?? string.Empty);
            }

            if (!string.IsNullOrEmpty(target.data.opendedChestModelData?.modelId))
            {
                var match = options.FirstOrDefault(o =>
                    o.id == target.data.opendedChestModelData.modelId
                );
                openedModelSelector.SetValueWithoutNotify(match.displayName ?? string.Empty);
            }
        }

        private void OnLootSelected(ChangeEvent<string> evt)
        {
            var lootTables = creatablesManager.GetAll<LootTable>();
            var selected = lootTables.FirstOrDefault(lt => lt.data.name == evt.newValue);
            if (selected != null)
                target.data.lootTableId = selected.data.id;
        }

        private async void OnClosedModelSelected(ChangeEvent<string> evt)
        {
            var options = placeablesLibrary.GetPlaceableOptions(
                PlaceableObjectCategories.Structure
            );
            var selected = options.FirstOrDefault(o => o.displayName == evt.newValue);
            await target.SetClosedModel(selected.id);
        }

        private async void OnOpenedModelSelected(ChangeEvent<string> evt)
        {
            var options = placeablesLibrary.GetPlaceableOptions(
                PlaceableObjectCategories.Structure
            );
            var selected = options.FirstOrDefault(o => o.displayName == evt.newValue);
            await target.SetOpenedModel(selected.id);
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

        private void ReinitializeGizmosForActiveTab()
        {
            Transform t = activeTab switch
            {
                "ClosedChest" => target.GetClosedModel()?.transform,
                "OpenedChest" => target.GetOpenedModel()?.transform,
                _ => target.transform,
            };

            if (t == null)
                return;
            if (positionGizmo != null)
                positionGizmo.Initialize(t, Camera.main);
            if (scaleGizmo != null)
                scaleGizmo.Initialize(t, Camera.main);
            if (rotationGizmo != null)
                rotationGizmo.Initialize(t, Camera.main);
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
            switch (activeTab)
            {
                case "Chest":
                    positionField.SetValueWithoutNotify(newPosition);
                    break;
                case "ClosedChest":
                    var localClosed = target.transform.InverseTransformPoint(newPosition);
                    target.data.closedChestModelData.relativePosition = localClosed;
                    closedPositionField.SetValueWithoutNotify(localClosed);
                    if (target.GetClosedModel() != null)
                        target.GetClosedModel().transform.localPosition = localClosed;
                    break;
                case "OpenedChest":
                    var localOpened = target.transform.InverseTransformPoint(newPosition);
                    target.data.opendedChestModelData.relativePosition = localOpened;
                    openedPositionField.SetValueWithoutNotify(localOpened);
                    if (target.GetOpenedModel() != null)
                        target.GetOpenedModel().transform.localPosition = localOpened;
                    break;
            }
        }

        private void OnGizmoScaled(Vector3 newScale)
        {
            switch (activeTab)
            {
                case "Chest":
                    scaleField.SetValueWithoutNotify(newScale);
                    break;
                case "ClosedChest":
                    target.data.closedChestModelData.relativeSize = newScale;
                    closedScaleField.SetValueWithoutNotify(newScale);
                    if (target.GetClosedModel() != null)
                        target.GetClosedModel().transform.localScale = newScale;
                    break;
                case "OpenedChest":
                    target.data.opendedChestModelData.relativeSize = newScale;
                    openedScaleField.SetValueWithoutNotify(newScale);
                    if (target.GetOpenedModel() != null)
                        target.GetOpenedModel().transform.localScale = newScale;
                    break;
            }
        }

        private void OnGizmoRotated(Vector3 eulerAngles)
        {
            switch (activeTab)
            {
                case "Chest":
                    rotationField.SetValueWithoutNotify(eulerAngles);
                    break;
                case "ClosedChest":
                    target.data.closedChestModelData.relativeRotation = eulerAngles;
                    closedRotationField.SetValueWithoutNotify(eulerAngles);
                    if (target.GetClosedModel() != null)
                        target.GetClosedModel().transform.localRotation = Quaternion.Euler(
                            eulerAngles
                        );
                    break;
                case "OpenedChest":
                    target.data.opendedChestModelData.relativeRotation = eulerAngles;
                    openedRotationField.SetValueWithoutNotify(eulerAngles);
                    if (target.GetOpenedModel() != null)
                        target.GetOpenedModel().transform.localRotation = Quaternion.Euler(
                            eulerAngles
                        );
                    break;
            }
        }

        // ---- Reset ----

        private void OnResetTransform()
        {
            switch (activeTab)
            {
                case "Chest":
                    rotationField.SetValueWithoutNotify(Vector3.zero);
                    scaleField.SetValueWithoutNotify(Vector3.one);
                    target.transform.localEulerAngles = Vector3.zero;
                    target.transform.localScale = Vector3.one;
                    break;
                case "ClosedChest":
                    closedPositionField.SetValueWithoutNotify(Vector3.zero);
                    closedRotationField.SetValueWithoutNotify(Vector3.zero);
                    closedScaleField.SetValueWithoutNotify(Vector3.one);
                    target.data.closedChestModelData.relativePosition = Vector3.zero;
                    target.data.closedChestModelData.relativeRotation = Vector3.zero;
                    target.data.closedChestModelData.relativeSize = Vector3.one;
                    if (target.GetClosedModel() != null)
                    {
                        target.GetClosedModel().transform.localPosition = Vector3.zero;
                        target.GetClosedModel().transform.localRotation = Quaternion.identity;
                        target.GetClosedModel().transform.localScale = Vector3.one;
                    }
                    break;
                case "OpenedChest":
                    openedPositionField.SetValueWithoutNotify(Vector3.zero);
                    openedRotationField.SetValueWithoutNotify(Vector3.zero);
                    openedScaleField.SetValueWithoutNotify(Vector3.one);
                    target.data.opendedChestModelData.relativePosition = Vector3.zero;
                    target.data.opendedChestModelData.relativeRotation = Vector3.zero;
                    target.data.opendedChestModelData.relativeSize = Vector3.one;
                    if (target.GetOpenedModel() != null)
                    {
                        target.GetOpenedModel().transform.localPosition = Vector3.zero;
                        target.GetOpenedModel().transform.localRotation = Quaternion.identity;
                        target.GetOpenedModel().transform.localScale = Vector3.one;
                    }
                    break;
            }
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

        private void OnRotateShortcut() => ActivateGizmo(rotationGizmo);

        private void OnHideShortcut() => ActivateGizmo(null);

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
            if (rotationGizmo != null)
            {
                rotationGizmo.OnRotationChanged -= OnGizmoRotated;
                rotationGizmo.gameObject.SetActive(false);
            }

            // restore both models to their default visibility
            var closed = target.GetClosedModel();
            var opened = target.GetOpenedModel();
            if (closed != null)
                closed.SetActive(true);
            if (opened != null)
                opened.SetActive(false);

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
            field == positionField || field == closedPositionField || field == openedPositionField
                ? positionScrollSensitivity
            : field == rotationField || field == closedRotationField || field == openedRotationField
                ? rotationScrollSensitivity
            : field == scaleField || field == closedScaleField || field == openedScaleField
                ? scaleScrollSensitivity
            : 1f;
    }
}
