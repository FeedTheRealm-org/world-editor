using System.Linq;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.Gameplay.WorldObjects;
using FTR.UI;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.PlaceableEditor
{
    [RequireComponent(typeof(UIDocument))]
    public class PlayerSpawnerEditor : MenuController, IEditable
    {
        private Vector3Field positionField;
        private Button closeButton;
        private Slider radiusSlider;
        private PlayerSpawnpointObject target;
        private PositionGizmo positionGizmo;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            radiusSlider = root.Q<Slider>("SpawnerRadius");
            positionField = root.Q<Vector3Field>("Position");

            radiusSlider.RegisterValueChangedCallback(e =>
            {
                float diameter = e.newValue * 2f;
                target.transform.localScale = new Vector3(
                    diameter,
                    target.transform.localScale.y,
                    diameter
                );
            });

            positionField.RegisterValueChangedCallback(e => target.transform.position = e.newValue);

            closeButton.clicked += CloseMenu;
        }

        public void Edit(GameObject placeable)
        {
            target = placeable.GetComponent<PlayerSpawnpointObject>();
            if (target == null)
            {
                Debug.LogError(
                    $"FriendlyNpcSpawnerEditor: {placeable.name} has no FriendlyNpcSpawnerObject component."
                );
                CloseMenu();
                return;
            }

            PopulateFields();
            SetupGizmo();
        }

        private void PopulateFields()
        {
            radiusSlider.SetValueWithoutNotify(target.transform.localScale.x / 2f);
            positionField.SetValueWithoutNotify(target.transform.position);
        }

        private void SetupGizmo()
        {
            positionGizmo = GetComponentInChildren<PositionGizmo>(includeInactive: true);
            if (positionGizmo == null)
                return;

            positionGizmo.Initialize(target.transform, Camera.main);
            positionGizmo.OnPositionChanged += OnGizmoMoved;
            positionGizmo.gameObject.SetActive(true);
        }

        private void OnGizmoMoved(Vector3 newPosition)
        {
            positionField.SetValueWithoutNotify(newPosition);
        }

        public override void CloseMenu()
        {
            base.CloseMenu();
            positionGizmo.OnPositionChanged -= OnGizmoMoved;
        }
    }
}
