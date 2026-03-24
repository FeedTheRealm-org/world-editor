using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldObjects
{
    public class StructureObject : WorldObjectController<StructureData>
    {
        private StructureData data = new();

        public override PlaceableObjectCategories Category => PlaceableObjectCategories.Structure;

        public override void SaveData(ref ZoneData worldData)
        {
            data.position = gameObject.transform.position;
            data.rotation = gameObject.transform.rotation.eulerAngles;
            data.size = gameObject.transform.localScale;
            BoxCollider collider = GetComponent<BoxCollider>();
            data.colliderSize = collider != null ? collider.size : Vector3.zero;
            data.colliderCenter = collider != null ? collider.center : Vector3.zero;
            worldData.objectPlacementData.Add(data);
        }

        public override void LoadData(StructureData data)
        {
            this.data = data;
            gameObject.name = data.structureName;
            gameObject.transform.position = data.position;
            gameObject.transform.rotation = Quaternion.Euler(data.rotation);
            gameObject.transform.localScale = data.size;
            FitColliderToMesh();
        }

        private void FitColliderToMesh()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return;

            // Merge all renderer bounds into one
            Bounds combined = renderers[0].bounds;
            foreach (var r in renderers)
                combined.Encapsulate(r.bounds);

            BoxCollider collider = GetComponent<BoxCollider>();

            // Convert world-space bounds to local space
            collider.center = transform.InverseTransformPoint(combined.center);
            collider.size = transform.InverseTransformVector(combined.size);
        }
    }
}
