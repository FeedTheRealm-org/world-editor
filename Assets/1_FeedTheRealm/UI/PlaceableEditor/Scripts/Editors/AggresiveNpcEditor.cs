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
    public class AggresiveNpcSpawnerEditor : MenuController, IEditable
    {
        [Inject]
        private CreatablesManager creatablesManager;

        private Slider radiusSlider;
        private IntegerField maxEnemiesField;
        private IntegerField spawnRateField;
        private IntegerField resetAfterKillsField;
        private IntegerField resetDelayField;
        private DropdownField enemyDropdown;
        private Button closeButton;
        private Vector3Field positionField;

        private AggresiveNpcSpawnerObject target;
        private PositionGizmo positionGizmo;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            radiusSlider = root.Q<Slider>("SpawnerRadius");
            maxEnemiesField = root.Q<IntegerField>("MaxEnemies");
            spawnRateField = root.Q<IntegerField>("SpawnRate");
            resetAfterKillsField = root.Q<IntegerField>("ResetAfterKills");
            resetDelayField = root.Q<IntegerField>("ResetDelay");
            enemyDropdown = root.Q<DropdownField>("EnemyDropdown");
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

            maxEnemiesField.RegisterValueChangedCallback(e => target.data.MaxEnemies = e.newValue);
            spawnRateField.RegisterValueChangedCallback(e => target.data.SpawnRate = e.newValue);
            resetAfterKillsField.RegisterValueChangedCallback(e =>
                target.data.ResetAfterKills = e.newValue
            );
            resetDelayField.RegisterValueChangedCallback(e => target.data.ResetDelay = e.newValue);

            enemyDropdown.RegisterValueChangedCallback(e =>
            {
                var selected = creatablesManager
                    .GetAll<AggresiveNpc>()
                    .FirstOrDefault(enemy => enemy.data.name == e.newValue);
                if (selected != null)
                    target.data.EnemyId = selected.Id;
            });

            positionField.RegisterValueChangedCallback(e => target.transform.position = e.newValue);

            closeButton.clicked += CloseMenu;
        }

        private void PopulateFields()
        {
            radiusSlider.SetValueWithoutNotify(target.transform.localScale.x / 2f);
            maxEnemiesField.SetValueWithoutNotify(target.data.MaxEnemies);
            spawnRateField.SetValueWithoutNotify((int)target.data.SpawnRate);
            resetAfterKillsField.SetValueWithoutNotify(target.data.ResetAfterKills);
            resetDelayField.SetValueWithoutNotify((int)target.data.ResetDelay);
            positionField.SetValueWithoutNotify(target.transform.position);

            var enemies = creatablesManager.GetAll<AggresiveNpc>();
            enemyDropdown.choices = enemies.Select(e => e.data.name).ToList();

            if (!string.IsNullOrEmpty(target.data.EnemyId))
            {
                var current = enemies.FirstOrDefault(e => e.Id == target.data.EnemyId);
                if (current != null)
                    enemyDropdown.SetValueWithoutNotify(current.data.name);
            }
        }

        public void Edit(GameObject placeable)
        {
            target = placeable.GetComponent<AggresiveNpcSpawnerObject>();
            if (target == null)
            {
                Debug.LogError(
                    $"AggresiveNpcSpawnerEditor: {placeable.name} has no AggresiveNpcSpawnerObject component."
                );
                Destroy(gameObject);
                return;
            }

            PopulateFields();
            SetupGizmo();
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
