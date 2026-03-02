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
        if (enableInputEvent != null)
            enableInputEvent.OnRaised += OnInputEventRaised;
        enableInputEvent?.Raise(false);
        enableEditorEvent?.Raise(false);
    }

    void OnDestroy()
    {
        if (enableInputEvent != null)
            enableInputEvent.OnRaised -= OnInputEventRaised;
    }

    private void OnInputEventRaised(bool isEnabled)
    {
        if (isEnabled)
            enableInputEvent?.Raise(false);
    }

    public void CloseMenu()
    {
        if (enableInputEvent != null)
            enableInputEvent.OnRaised -= OnInputEventRaised;
        enableInputEvent?.Raise(true);
        enableEditorEvent?.Raise(true);
        Destroy(gameObject);
    }

    public void OpenMenu(GameObject menuPrefab)
    {
        resolver.Instantiate(menuPrefab);
        Destroy(gameObject);
    }
}
