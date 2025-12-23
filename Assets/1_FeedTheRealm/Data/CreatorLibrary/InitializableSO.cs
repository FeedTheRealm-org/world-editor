using UnityEngine;

public abstract class InitializableSO : ScriptableObject {

    private bool _initialized;

    public void Initialize() {
        if (_initialized) return;

        _initialized = true;
        OnInitialize();
    }

    protected abstract void OnInitialize();

    public void Reset() {
        _initialized = false;
        OnReset();
    }

    protected virtual void OnReset() { }
}
