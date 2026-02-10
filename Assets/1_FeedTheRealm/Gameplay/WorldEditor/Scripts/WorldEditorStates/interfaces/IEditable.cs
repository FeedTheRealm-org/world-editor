using System;

public interface IEditable
{
    void OnObjectSelected(Action CloseEditorCallback);

    void OnObjectDeselected();
}
