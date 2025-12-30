using UnityEngine;
using UnityEngine.UIElements;
using Models;

public class EnemySpawnAreaHUDController : MonoBehaviour {
    UIDocument document;
    VisualElement root;

    IntegerField maxEnemies;
    FloatField spawnRate;
    IntegerField resetKills;
    FloatField resetDelay;
    Button closeButton;
    Button editButton;

    EnemySpawnPlace spawnerPlace;

    void OnEnable() {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;

        maxEnemies = root.Q<IntegerField>("MaxEnemiesInput");
        spawnRate = root.Q<FloatField>("SpawnRateInput");
        resetKills = root.Q<IntegerField>("ResetAfterKillsInput");
        resetDelay = root.Q<FloatField>("ResetDelayInput");
        closeButton = root.Q<Button>("Close");
        editButton = root.Q<Button>("Edit");

        closeButton.clicked += Hide;
        editButton.clicked += Edit;
    }

    public void Show(EnemySpawnPlace spawnerPlace) {
        this.spawnerPlace = spawnerPlace;

        maxEnemies.value = this.spawnerPlace.spawnData.MaxEnemies;
        spawnRate.value = this.spawnerPlace.spawnData.SpawnRate;
        resetKills.value = this.spawnerPlace.spawnData.ResetAfterKills;
        resetDelay.value = this.spawnerPlace.spawnData.ResetDelay;
    }

    public void Edit() {
        spawnerPlace.NotifyChanges(new EnemySpawnAreaData(
            spawnerPlace.transform.position,
            1,
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
