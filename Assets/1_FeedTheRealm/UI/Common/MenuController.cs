using System;
using UnityEngine;
using Utils;

public class MenuController : MonoBehaviour
{
    void Awake()
    {
        SelectionRaiser.RaiseEnableInput(false);
        SelectionRaiser.RaiseEnableEditor(false);
    }

    public void CloseMenu()
    {
        SelectionRaiser.RaiseEnableInput(true);
        SelectionRaiser.RaiseEnableEditor(true);
        Destroy(gameObject);
    }

    public void OpenMenu(GameObject menuprefab)
    {
        CloseMenu();
        Instantiate(menuprefab);
    }
}
