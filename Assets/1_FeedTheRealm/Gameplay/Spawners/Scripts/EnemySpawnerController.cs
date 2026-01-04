using Models;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class EnemySpawnerController : SpawnerController, IPersistent, ISelectable
{
    [SerializeField]
    private MakerInputReader inputReader;

    private UIDocument editorMenu;

    private Slider radiusSlider;
    private IntegerField maxEnemiesField;
    private IntegerField spawnRateField;
    private IntegerField resetAfterKillsField;
    private IntegerField resetDelayField;
    private Button closeButton;
    private EnemySpawnAreaData _enemySpawnData;

    public EnemySpawnAreaData enemySpawnData
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

    void Start()
    {
        enemySpawnData = new EnemySpawnAreaData(transform.position, transform.localScale.x);
        InitUi();
    }

    private void InitUi()
    {
        editorMenu = GetComponent<UIDocument>();
        var root = editorMenu.rootVisualElement;

        radiusSlider = root.Q<Slider>("SpawnerRadius");
        maxEnemiesField = root.Q<IntegerField>("MaxEnemies");
        spawnRateField = root.Q<IntegerField>("SpawnRate");
        resetAfterKillsField = root.Q<IntegerField>("ResetAfterKills");
        resetDelayField = root.Q<IntegerField>("ResetDelay");
        closeButton = root.Q<Button>("Close");

        radiusSlider.RegisterValueChangedCallback(e =>
        {
            enemySpawnData.Radius = e.newValue;
            transform.localScale = new Vector3(e.newValue, transform.localScale.y, e.newValue);
        });
        maxEnemiesField.RegisterValueChangedCallback(e => enemySpawnData.MaxEnemies = e.newValue);
        spawnRateField.RegisterValueChangedCallback(e => enemySpawnData.SpawnRate = e.newValue);
        resetAfterKillsField.RegisterValueChangedCallback(e =>
            enemySpawnData.ResetAfterKills = e.newValue
        );
        resetDelayField.RegisterValueChangedCallback(e => enemySpawnData.ResetDelay = e.newValue);
        closeButton.clicked += OnCloseClicked;

        editorMenu.rootVisualElement.style.display = DisplayStyle.None;
    }

    private void RenderMenu()
    {
        inputReader.ToggleInput(false);
        radiusSlider.value = enemySpawnData.Radius;
        maxEnemiesField.value = enemySpawnData.MaxEnemies;
        spawnRateField.value = (int)enemySpawnData.SpawnRate;
        resetAfterKillsField.value = enemySpawnData.ResetAfterKills;
        resetDelayField.value = (int)enemySpawnData.ResetDelay;

        editorMenu.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    private void OnCloseClicked()
    {
        Debug.Log("EnemySpawnerController: Closing editor menu");
        inputReader.ToggleInput(true);
        editorMenu.rootVisualElement.style.display = DisplayStyle.None;
    }

    public void OnObjectSelected()
    {
        RenderMenu();
    }

    public override void SaveData(ref WorldData worldData)
    {
        if (!gameObject.activeSelf)
            return;

        worldData.enemySpawnAreas.Add(enemySpawnData);
    }
}
