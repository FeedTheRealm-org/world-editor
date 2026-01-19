using System;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private MakerInputReader inputReader;

    public Action<bool> ToggleEditorCallback;

    void Awake()
    {
        inputReader.ToggleInput(false);
    }

    public void CloseMenu()
    {
        inputReader.ToggleInput(true);
        ToggleEditorCallback.Invoke(true);
        Destroy(gameObject);
    }

    public void OpenMenu(GameObject menuprefab)
    {
        CloseMenu();
        Instantiate(menuprefab);
    }
}
