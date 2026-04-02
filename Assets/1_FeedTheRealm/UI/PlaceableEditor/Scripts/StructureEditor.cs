using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Gameplay.Inputs;
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

        [SerializeField]
        private float positionScrollSensitivity = 1f;

        [SerializeField]
        private float rotationScrollSensitivity = 10f;

        [SerializeField]
        private float scaleScrollSensitivity = 1f;

        // UI Elements
        private Label titleLabel;
        private Vector3Field positionField;
        private Vector3Field rotationField;
        private Vector3Field scaleField;
        private Button closeButton;

        private FloatField focusedAxisField;
        private Vector3Field focusedVectorField;

        // Target
        private StructureObject target;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            titleLabel = root.Q<Label>("StructureName");
            positionField = root.Q<Vector3Field>("Position");
            rotationField = root.Q<Vector3Field>("Rotation");
            scaleField = root.Q<Vector3Field>("Scale");
            closeButton = root.Q<Button>("Close");

            positionField.RegisterValueChangedCallback(e => target.transform.position = e.newValue);
            rotationField.RegisterValueChangedCallback(e =>
                target.transform.localEulerAngles = e.newValue
            );
            scaleField.RegisterValueChangedCallback(e => target.transform.localScale = e.newValue);

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
                    $"PlaceableEditorController: {placeable.name} has no StructureObject component."
                );
                Destroy(gameObject);
                return;
            }

            titleLabel.text = target.name;
            positionField.SetValueWithoutNotify(target.transform.position);
            rotationField.SetValueWithoutNotify(target.transform.localEulerAngles);
            scaleField.SetValueWithoutNotify(target.transform.localScale);

            inputReader.ScrollEvent += OnScroll;
        }

        public override void CloseMenu()
        {
            inputReader.ScrollEvent -= OnScroll;
            focusedAxisField = null;
            focusedVectorField = null;
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
