using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Core.WorldObjects.Enemies;
using FeedTheRealm.Gameplay.Library.CreatorObjectLibrary;
using FeedTheRealm.Gameplay.WorldObjects;
using FeedTheRealm.UI.Common;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.PlaceableEditor
{
    [RequireComponent(typeof(UIDocument))]
    public class AggresiveNpcSpawnerEditor : MenuController, IEditable
    {
        // [Inject] private CreatorObjectLibrarySO creatorObjectLibrary;
        private Slider radiusSlider;
        private IntegerField maxEnemiesField;
        private IntegerField spawnRateField;
        private IntegerField resetAfterKillsField;
        private IntegerField resetDelayField;
        private DropdownField enemyDropdown;
        private Button closeButton;

        private AggresiveNpcSpawnerObject target;

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

            radiusSlider.RegisterValueChangedCallback(e =>
            {
                target.data.Radius = e.newValue;
                target.transform.localScale = new Vector3(
                    e.newValue,
                    target.transform.localScale.y,
                    e.newValue
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
                var enemies = GetEnemies();
                var selected = enemies.FirstOrDefault(enemy => enemy.DisplayName == e.newValue);
                if (selected != null)
                    target.data.EnemyId = selected.ObjectId;
            });
            closeButton.clicked += CloseMenu;
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
        }

        private void PopulateFields()
        {
            radiusSlider.SetValueWithoutNotify(target.data.Radius);
            maxEnemiesField.SetValueWithoutNotify(target.data.MaxEnemies);
            spawnRateField.SetValueWithoutNotify((int)target.data.SpawnRate);
            resetAfterKillsField.SetValueWithoutNotify(target.data.ResetAfterKills);
            resetDelayField.SetValueWithoutNotify((int)target.data.ResetDelay);

            var enemies = GetEnemies();
            enemyDropdown.choices = enemies.Select(e => e.DisplayName).ToList();

            if (!string.IsNullOrEmpty(target.data.EnemyId))
            {
                var current = enemies.FirstOrDefault(e => e.ObjectId == target.data.EnemyId);
                if (current != null)
                    enemyDropdown.SetValueWithoutNotify(current.DisplayName);
            }
        }

        private List<GenericEnemy> GetEnemies() => new();
        // creatorObjectLibrary
        //     .GetCreatables(CreatorObjectCategories.Enemy)
        //     .Cast<GenericEnemy>()
        //     .ToList();
    }
}
