using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Models;
using System.Drawing;

[RequireComponent(typeof(UIDocument))]
public class EnemySpawnAreaHUDController : MonoBehaviour {
    UIDocument document;
    VisualElement root;

    FloatField size;
    IntegerField maxEnemies;
    FloatField spawnRate;
    IntegerField resetKills;
    FloatField resetDelay;
    Button closeButton;
    Button editButton;
    DropdownField enemyDropdown;

    EnemySpawnPlace spawnerPlace;
    [SerializeField] private Enemy EnemyDatabase;

    void OnEnable() {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;

        size = root.Q<FloatField>("SizeInput");
        maxEnemies = root.Q<IntegerField>("MaxEnemiesInput");
        spawnRate = root.Q<FloatField>("SpawnRateInput");
        resetKills = root.Q<IntegerField>("ResetAfterKillsInput");
        resetDelay = root.Q<FloatField>("ResetDelayInput");
        closeButton = root.Q<Button>("Close");
        editButton = root.Q<Button>("Edit");
        enemyDropdown = root.Q<DropdownField>("EnemyForSpawnDropdown");

        closeButton.clicked += Hide;
        editButton.clicked += Edit;

        PopulateEnemyDropdown();
    }

    public void Show(EnemySpawnPlace spawnerPlace) {
        this.spawnerPlace = spawnerPlace;

        size.value = this.spawnerPlace.spawnData.Size;
        maxEnemies.value = this.spawnerPlace.spawnData.MaxEnemies;
        spawnRate.value = this.spawnerPlace.spawnData.SpawnRate;
        resetKills.value = this.spawnerPlace.spawnData.ResetAfterKills;
        resetDelay.value = this.spawnerPlace.spawnData.ResetDelay;
    }

    void PopulateEnemyDropdown() {
        if (enemyDropdown == null) return;

        var choices = new List<string>();
        
        if (EnemyDatabase != null) {
            var allEnemies = EnemyDatabase.GetAllEnemies();
            foreach (var enemy in allEnemies) {
                if (enemy != null && !string.IsNullOrEmpty(enemy.name)) {
                    choices.Add(enemy.name);
                }
            }
        }

        choices.Add("Default");
        enemyDropdown.choices = choices;
        enemyDropdown.value = "Default";
    }

    public void Edit() {
        spawnerPlace.NotifyChanges(new EnemySpawnAreaData(
            spawnerPlace.transform.position,
            size.value,
            maxEnemies.value,
            spawnRate.value,
            resetKills.value,
            resetDelay.value
        ));
        Hide();
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
