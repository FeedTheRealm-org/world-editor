using System;
using System.Linq;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.Interfaces;
using FeedTheRealm.Core.WorldObjects.Enemies;
using FeedTheRealm.Core.WorldObjects.Items;
using FeedTheRealm.Gameplay.Inputs;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class EnemySpawnerController : SpawnerController, IPersistent, IEditable
{
    [SerializeField]
    private InputReader inputReader;
    private UIDocument editorMenu;
    private Slider radiusSlider;
    private IntegerField maxEnemiesField;
    private IntegerField spawnRateField;
    private IntegerField resetAfterKillsField;
    private IntegerField resetDelayField;
    private Button closeButton;
    private EnemySpawnerData _enemySpawnData;
    private DropdownField enemyDropdown;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    public EnemySpawnerData EnemySpawnData
    {
        get { return _enemySpawnData; }
        set
        {
            _enemySpawnData = value;
            transform.position = _enemySpawnData.Position;
            transform.localScale = new Vector3(
                _enemySpawnData.Radius,
                transform.localScale.y,
                _enemySpawnData.Radius
            );
        }
    }

    public void OnObjectSelected(Action CloseEditorCallback) => RenderMenu(CloseEditorCallback);

    public void OnObjectDeselected()
    {
        CloseMenu();
    }

    void OnEnable()
    {
        EnemySpawnData = new EnemySpawnerData(transform.position, transform.localScale.x);
        editorMenu = GetComponent<UIDocument>();
        var root = editorMenu.rootVisualElement;

        radiusSlider = root.Q<Slider>("SpawnerRadius");
        maxEnemiesField = root.Q<IntegerField>("MaxEnemies");
        spawnRateField = root.Q<IntegerField>("SpawnRate");
        resetAfterKillsField = root.Q<IntegerField>("ResetAfterKills");
        resetDelayField = root.Q<IntegerField>("ResetDelay");
        closeButton = root.Q<Button>("Close");
        enemyDropdown = root.Q<DropdownField>("EnemyDropdown");

        radiusSlider.value = EnemySpawnData.Radius;
        maxEnemiesField.value = EnemySpawnData.MaxEnemies;
        spawnRateField.value = (int)EnemySpawnData.SpawnRate;
        resetAfterKillsField.value = EnemySpawnData.ResetAfterKills;
        resetDelayField.value = (int)EnemySpawnData.ResetDelay;

        radiusSlider.RegisterValueChangedCallback(e =>
        {
            EnemySpawnData.Radius = e.newValue;
            transform.localScale = new Vector3(e.newValue, transform.localScale.y, e.newValue);
        });
        maxEnemiesField.RegisterValueChangedCallback(e => EnemySpawnData.MaxEnemies = e.newValue);
        spawnRateField.RegisterValueChangedCallback(e => EnemySpawnData.SpawnRate = e.newValue);
        resetAfterKillsField.RegisterValueChangedCallback(e =>
            EnemySpawnData.ResetAfterKills = e.newValue
        );
        resetDelayField.RegisterValueChangedCallback(e => EnemySpawnData.ResetDelay = e.newValue);
        editorMenu.rootVisualElement.style.display = DisplayStyle.None;
    }

    private void RenderMenu(Action CloseEditorCallback)
    {
        inputReader.ToggleInput(false);
        radiusSlider.value = EnemySpawnData.Radius;
        maxEnemiesField.value = EnemySpawnData.MaxEnemies;
        spawnRateField.value = (int)EnemySpawnData.SpawnRate;
        resetAfterKillsField.value = EnemySpawnData.ResetAfterKills;
        resetDelayField.value = (int)EnemySpawnData.ResetDelay;

        if (enemyDropdown != null && creatorObjectLibrary != null)
        {
            var enemies = creatorObjectLibrary
                .GetCreatables(CreatorObjectCategories.Enemy)
                .Cast<GenericEnemy>()
                .ToList();

            enemyDropdown.choices = enemies.Select(enemy => enemy.DisplayName).ToList();

            if (!string.IsNullOrEmpty(EnemySpawnData.EnemyId))
            {
                var selectedEnemy = enemies.FirstOrDefault(enemy =>
                    enemy.ObjectId == EnemySpawnData.EnemyId
                );
                if (selectedEnemy != null)
                {
                    enemyDropdown.value = selectedEnemy.DisplayName;
                }
            }

            enemyDropdown.RegisterValueChangedCallback(e =>
            {
                var selectedEnemy = enemies.FirstOrDefault(enemy =>
                    enemy.DisplayName == e.newValue
                );
                if (selectedEnemy != null)
                {
                    EnemySpawnData.EnemyId = selectedEnemy.ObjectId;
                    Debug.Log(
                        $"EnemySpawnerController: Selected Enemy '{selectedEnemy.DisplayName}' (ID: {selectedEnemy.ObjectId})"
                    );
                }
            });
        }

        editorMenu.rootVisualElement.style.display = DisplayStyle.Flex;
        closeButton.clicked += () =>
        {
            CloseMenu();
            CloseEditorCallback?.Invoke();
        };
    }

    private void CloseMenu()
    {
        Debug.Log("EnemySpawnerController: Closing editor menu");
        inputReader.ToggleInput(true);
        editorMenu.rootVisualElement.style.display = DisplayStyle.None;
    }

    public override void SaveData(ref WorldData worldData)
    {
        if (!gameObject.activeSelf)
            return;

        _enemySpawnData.Position = transform.position;
        worldData.enemySpawnAreas.Add(EnemySpawnData);
    }
}
