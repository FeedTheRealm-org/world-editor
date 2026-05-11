using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldObjects
{
    public class StructureObject : Placeable<StructureData>
    {
        public override PlaceableObjectCategories Category => PlaceableObjectCategories.Structure;

        public override void SaveData(ref ZoneData zoneData)
        {
            var boxCollider = GetComponent<BoxCollider>();

            StructureData savedData = new()
            {
                id = data.id,
                structureName = data.structureName,
                fileName = data.fileName,
                isShop = data.isShop,
                shopId = data.shopId,
                hasColliders = data.hasColliders,
                position = gameObject.transform.position,
                rotation = gameObject.transform.rotation.eulerAngles,
                size = gameObject.transform.localScale,
                colliderCenter =
                    data.colliderCenter != Vector3.zero ? data.colliderCenter : boxCollider.center,
                colliderSize =
                    data.colliderSize != Vector3.zero ? data.colliderSize : boxCollider.size,
                colliderRotation = data.colliderRotation,
                colliderType = data.colliderType,
            };
            zoneData.objectPlacementData.Add(savedData);
        }

        public override void LoadData(StructureData data)
        {
            this.data = data.Clone();
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

            Bounds combined = renderers[0].bounds;
            foreach (var r in renderers)
                combined.Encapsulate(r.bounds);

            BoxCollider collider = GetComponent<BoxCollider>();
            collider.center = transform.InverseTransformPoint(combined.center);

            Vector3 scale = transform.lossyScale;
            collider.size = new Vector3(
                combined.size.x / scale.x,
                combined.size.y / scale.y,
                combined.size.z / scale.z
            );
        }
    }
}
