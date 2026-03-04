using FeedTheRealm.Core.DataPersistence;
using Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.Spawners
{
    public class SpawnerController : MonoBehaviour, IPersistent
    {
        [SerializeField]
        private Color spawnerColor = Color.white;

        void Awake()
        {
            ApplyColor();
        }

        void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;
            ApplyColor();
        }

        private void ApplyColor()
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = spawnerColor;
            }
        }

        public virtual void SaveData(ref WorldData worldData)
        {
            throw new System.NotImplementedException();
        }
    }
}
