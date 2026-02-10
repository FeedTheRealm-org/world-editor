using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class EnemiesMenuController : MenuController
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private GameObject createEnemyMenuPrefab;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    [SerializeField]
    private VisualTreeAsset itemListTemplate;
    private Button closeButton;
    private Button addEnemyButton;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        closeButton = root.Q<Button>("Close");
        addEnemyButton = root.Q<Button>("AddEnemy");

        addEnemyButton.clicked += AddEnemy;
        closeButton.clicked += CloseMenu;

        PopulateEnemysList();
    }

    private void PopulateEnemysList()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var enemiesList = root.Q<ListView>("EnemiesList");
        enemiesList.Clear();

        foreach (
            GenericEnemy enemy in creatorObjectLibrary.GetCreatables(CreatorObjectCategories.Enemy)
        )
        {
            VisualElement enemyEntry = itemListTemplate.Instantiate();
            var headerLabel = enemyEntry.Q<Label>("Header");
            headerLabel.text = enemy.DisplayName;

            var editButton = enemyEntry.Q<Button>("Edit");
            var deleteButton = enemyEntry.Q<Button>("Delete");

            editButton.clicked += () => OnEditEnemy(enemy);
            deleteButton.clicked += () => OnDeleteEnemy(enemy, enemyEntry);

            enemiesList.hierarchy.Add(enemyEntry);
        }
    }

    void OnEditEnemy(CreatorObject enemy)
    {
        logger.Log("Editing enemy: " + enemy.DisplayName, this, Logging.LogType.Info);

        EditContext.SetObjectToEdit(enemy);

        OpenMenu(createEnemyMenuPrefab);
    }

    void OnDeleteEnemy(CreatorObject enemy, VisualElement enemyListEntry)
    {
        logger.Log("Deleting enemy: " + enemy.DisplayName, this, Logging.LogType.Info);
        creatorObjectLibrary.RemoveCreatable(CreatorObjectCategories.Enemy, enemy);
        enemyListEntry.RemoveFromHierarchy();
    }

    void OnDisable()
    {
        addEnemyButton.clicked -= AddEnemy;
        closeButton.clicked -= CloseMenu;
    }

    private void AddEnemy()
    {
        logger.Log("Opening Create Enemy Menu", this, Logging.LogType.Info);
        OpenMenu(createEnemyMenuPrefab);
    }
}
