using System.IO;
using UnityEngine;

namespace FTR.Core.Common.Config
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Config/Config")]
    public class Config : ScriptableObject
    {
        [Header("Reinitialization Options")]
        [SerializeField]
        private bool forceReinitialize = false;
        public bool ForceReinitialize => forceReinitialize;

        [Header("Persistent file paths")]
        [SerializeField]
        private readonly string modelsDataFile = "Models/models.json";

        [SerializeField]
        private readonly string modelsDirectory = "Models";

        public string ModelsDataFile =>
            Path.Combine(Application.persistentDataPath, modelsDataFile);
        public string ModelsDirectory =>
            Path.Combine(Application.streamingAssetsPath, modelsDirectory);
    }
}
