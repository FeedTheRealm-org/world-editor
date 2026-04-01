using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldObjects
{
    public class StructureObject : Placeable<StructureData>
    {
        private StructureData data = new();

        public override PlaceableObjectCategories Category => PlaceableObjectCategories.Structure;

        public override void SaveData(ref ZoneData zoneData)
        {
            BoxCollider collider = GetComponent<BoxCollider>();
            StructureData data = new()
            {
                id = this.data.id,
                fileName = this.data.fileName,
                structureName = gameObject.name,
                position = gameObject.transform.position,
                rotation = gameObject.transform.rotation.eulerAngles,
                size = gameObject.transform.localScale,
                colliderSize = collider != null ? collider.size : Vector3.zero,
                colliderCenter = collider != null ? collider.center : Vector3.zero,
            };
            zoneData.objectPlacementData.Add(data);
        }

        public override void LoadData(StructureData data)
        {
            this.data = data.Clone();
            gameObject.name = data.structureName;
            gameObject.transform.position = data.position;
            gameObject.transform.rotation = Quaternion.Euler(data.rotation);
            gameObject.transform.localScale = data.size;
            FitColliderToMesh();
            this.data = data;
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
