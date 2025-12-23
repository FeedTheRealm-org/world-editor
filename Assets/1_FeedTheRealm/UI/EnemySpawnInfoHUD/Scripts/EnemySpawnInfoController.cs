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

    void OnEnable() {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;

        maxEnemies = root.Q<IntegerField>("MaxEnemiesInput");
        spawnRate = root.Q<FloatField>("SpawnRateInput");
        resetKills = root.Q<IntegerField>("ResetAfterKillsInput");
        resetDelay = root.Q<FloatField>("ResetDelayInput");
        closeButton = root.Q<Button>("Close");

        closeButton.clicked += Hide;
    }

    public void Show(EnemySpawnAreaData spawnData) {
        maxEnemies.value = spawnData.MaxEnemies;
        spawnRate.value = spawnData.SpawnRate;
        resetKills.value = spawnData.ResetAfterKills;
        resetDelay.value = spawnData.ResetDelay;

        maxEnemies.RegisterValueChangedCallback(e => spawnData.MaxEnemies = e.newValue);
        spawnRate.RegisterValueChangedCallback(e => spawnData.SpawnRate = e.newValue);
        resetKills.RegisterValueChangedCallback(e => spawnData.ResetAfterKills = e.newValue);
        resetDelay.RegisterValueChangedCallback(e => spawnData.ResetDelay = e.newValue);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
