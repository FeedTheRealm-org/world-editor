using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MenuController : MonoBehaviour
{
    [Inject]
    protected EnableInputEvent enableInputEvent;

    [Inject]
    protected EnableEditorEvent enableEditorEvent;

    [Inject]
    private IObjectResolver resolver;

    void Awake()
    {
        enableInputEvent?.Raise(false);
        enableEditorEvent?.Raise(false);
    }

    public void CloseMenu()
    {
        enableInputEvent?.Raise(true);
        enableEditorEvent?.Raise(true);
        Destroy(gameObject);
    }

    public void OpenMenu(GameObject menuPrefab)
    {
        enableInputEvent?.Raise(true);
        enableEditorEvent?.Raise(true);
        resolver.Instantiate(menuPrefab);
        Destroy(gameObject);
    }
}
