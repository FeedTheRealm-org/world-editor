using System;
using Unity.VisualScripting;
using UnityEngine;
using Models;

public class EnemySpawnPlace : MonoBehaviour {
    [SerializeField] Renderer[] renderers;
    [SerializeField] Color highlightColor = Color.blue;

    [SerializeField] GameObject enemySpawnHUD;

    [SerializeField] public EnemySpawnAreaData spawnData;

    Color[] originalColors;

    void Awake() {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].material.color;
    }

    public void Highlight(bool value) {
        for (int i = 0; i < renderers.Length; i++) {
            renderers[i].material.color =
                value ? highlightColor : originalColors[i];
        }
    }

    public void OpenHUD() {
        if (enemySpawnHUD != null) {
            enemySpawnHUD.SetActive(true);
            enemySpawnHUD.GetComponent<EnemySpawnAreaHUDController>().Show(this);
        }
    }

    public void NotifyChanges(EnemySpawnAreaData newData) {
        spawnData = newData;
    }

    public EnemySpawnAreaData GetData() {
        return spawnData;
    }
}
