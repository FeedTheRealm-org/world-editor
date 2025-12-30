using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuBarController : MonoBehaviour
{
    [SerializeField]
    private List<MenuOptions> menuOptions;
    private VisualElement menuBar;

    void Start()
    {
        UIDocument hudVisualDocument = GetComponent<UIDocument>();
        menuBar = hudVisualDocument.rootVisualElement.Q<VisualElement>("MenuBar");
        RenderMenuButtons();
    }

    private void RenderMenuButtons()
    {
        foreach (var option in menuOptions)
        {
            GameObject menuPanel = Instantiate(option.panel);
            menuPanel.SetActive(false);
            MenuController menuController =
                menuPanel.GetComponent<MenuController>()
                ?? throw new InvalidOperationException(
                    $"MenuBarController: The panel for menu option '{option.Name}' is not a MenuObject component."
                );
            var menuButton = new Button() { text = option.Name };
            menuButton.clicked += () =>
            {
                menuController.OpenMenu();
            };
            menuButton.AddToClassList("menuButtons");
            menuBar.Add(menuButton);
        }
    }
}

[System.Serializable]
public class MenuOptions
{
    public string Name;
    public GameObject panel;
}
