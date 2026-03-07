using System;

namespace FeedTheRealm.Core.Interfaces
{
    public interface IEditable
    {
        void OnObjectSelected(Action CloseEditorCallback);

        void OnObjectDeselected();
    }
}
