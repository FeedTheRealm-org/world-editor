using System;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private MakerInputReader inputReader;

    [SerializeField]
    private WorldEditorStateMachine worldEditorStateMachine;

    public Action<bool> ToggleEditorCallback;

    void Awake()
    {
        inputReader.ToggleInput(false);
        worldEditorStateMachine?.ToggleEditor(false);
    }

    public void CloseMenu()
    {
        inputReader.ToggleInput(true);
        worldEditorStateMachine?.ToggleEditor(true);
        Destroy(gameObject);
    }

    public void OpenMenu(GameObject menuprefab)
    {
        CloseMenu();
        Instantiate(menuprefab);
    }
}
