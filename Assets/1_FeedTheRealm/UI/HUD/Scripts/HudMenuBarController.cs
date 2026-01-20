using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuBarController : MonoBehaviour
{
    [SerializeField]
    private WorldEditorStateMachine worldEditorStateMachine;

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
            var menuButton = new Button() { text = option.Name };
            menuButton.clicked += () =>
            {
                GameObject menuPanel = Instantiate(option.panel);
                MenuController menuController = menuPanel.GetComponent<MenuController>();
                menuController.ToggleEditorCallback += worldEditorStateMachine.ToggleEditor;
                worldEditorStateMachine.ToggleEditor(false);
            };
            menuButton.AddToClassList("menuButtons");
            menuBar.Add(menuButton);
        }
    }
}

[Serializable]
public class MenuOptions
{
    public string Name;
    public GameObject panel;
}
