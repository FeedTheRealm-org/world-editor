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
    public class PassiveNpcSpawnerEditor : MenuController, IEditable
    {
        [Inject]
        private CreatablesManager CreatablesManager;

        private Slider radiusSlider;
        private DropdownField npcDropdown;
        private Button closeButton;
        private Vector3Field positionField;

        private FriendlyNpcSpawnerObject target;
        private PositionGizmo positionGizmo;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            radiusSlider = root.Q<Slider>("SpawnerRadius");
            npcDropdown = root.Q<DropdownField>("NPCDropdown");
            closeButton = root.Q<Button>("Close");
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

            npcDropdown.RegisterValueChangedCallback(e =>
            {
                var selected = CreatablesManager
                    .GetAll<FriendlyNpc>()
                    .FirstOrDefault(npc => npc.data.name == e.newValue);
                if (selected != null)
                    target.data.NpcId = selected.Id;
            });

            positionField.RegisterValueChangedCallback(e => target.transform.position = e.newValue);

            closeButton.clicked += CloseMenu;
        }

        public void Edit(GameObject placeable)
        {
            target = placeable.GetComponent<FriendlyNpcSpawnerObject>();
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

            var npcs = CreatablesManager.GetAll<FriendlyNpc>();
            npcDropdown.choices = npcs.Select(n => n.data.name).ToList();

            if (!string.IsNullOrEmpty(target.data.NpcId))
            {
                var current = npcs.FirstOrDefault(n => n.Id == target.data.NpcId);
                if (current != null)
                    npcDropdown.SetValueWithoutNotify(current.data.name);
            }
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
