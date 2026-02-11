using System;
using UnityEngine;
using Utils;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private WorldEditorStateMachine worldEditorStateMachine;

    public Action<bool> ToggleEditorCallback;

    void Awake()
    {
        SelectionRaiser.RaiseEnableInput(false);
    }

    public void CloseMenu()
    {
        SelectionRaiser.RaiseEnableInput(true);
        Destroy(gameObject);
    }

    public void OpenMenu(GameObject menuprefab)
    {
        CloseMenu();
        Instantiate(menuprefab);
    }
}
