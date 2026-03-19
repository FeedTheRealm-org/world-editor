using FeedTheRealm.Core.WorldObjects;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldObjects
{
    public class StructureObject : WorldObjectController, ILoadable<StructureData>
    {
        private StructureData data = new();

        public void Load(StructureData data)
        {
            this.data = data;
            Setup();
        }

        private void Setup()
        {
            gameObject.name = data.structureName;
            gameObject.transform.position = data.position;
            gameObject.transform.rotation = Quaternion.Euler(data.rotation);
            gameObject.transform.localScale = data.size;
        }

        public override void SaveData(ref WorldData worldData)
        {
            data.position = gameObject.transform.position;
            data.rotation = gameObject.transform.rotation.eulerAngles;
            data.size = gameObject.transform.localScale;
            BoxCollider collider = GetComponent<BoxCollider>();
            data.colliderSize = collider != null ? collider.size : Vector3.zero;
            data.colliderCenter = collider != null ? collider.center : Vector3.zero;
            worldData.objectPlacementData.Add(data);
        }
    }
}
