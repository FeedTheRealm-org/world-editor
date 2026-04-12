using UnityEngine;

namespace FeedTheRealm.UI.Common
{
    /// <summary>
    /// Helps separate the UI visualization values from the actual data in memory.
    /// It effectively clones the data using Unity's JsonUtility when editing starts.
    /// When you want to save, call Commit() to overwrite the original data.
    /// This prevents Close or Return buttons from accidentally saving real-time changes.
    /// </summary>
    public class EditBuffer<T>
        where T : class
    {
        public T Original { get; private set; }
        public T Working { get; private set; }

        public EditBuffer(T data)
        {
            Original = data;
            // Create a deep copy of the serializable fields
            string json = JsonUtility.ToJson(data);
            Working = JsonUtility.FromJson<T>(json);
        }

        public void Commit()
        {
            if (Original != null && Working != null)
            {
                // Overwrite the original object with the fields from the working copy
                string json = JsonUtility.ToJson(Working);
                JsonUtility.FromJsonOverwrite(json, Original);
            }
        }
    }
}
