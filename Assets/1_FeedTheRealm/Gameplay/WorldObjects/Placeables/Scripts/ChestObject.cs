using System;
using System.Threading.Tasks;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FTRShared.Runtime.Models;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Gameplay.WorldObjects
{
    public class ChestObject : Placeable<ChestData>
    {
        [Inject]
        private PlaceablesLibrary placeablesLibrary;

        public override PlaceableObjectCategories Category => PlaceableObjectCategories.Chest;

        private GameObject openChestModel;
        private GameObject closedChestModel;

        private async void Start()
        {
            if (data == null || string.IsNullOrEmpty(data.id))
            {
                data = new ChestData
                {
                    id = Guid.NewGuid().ToString(),
                    name = "Chest",
                    closedChestModelData = new ChestModelData
                    {
                        modelId = config.defaultClosedChestId,
                    },
                    opendedChestModelData = new ChestModelData
                    {
                        modelId = config.defaultOpenChestId,
                    },
                };
            }

            closedChestModel = await SetModel(data.closedChestModelData.modelId);
            closedChestModel.transform.localPosition = data.closedChestModelData.relativePosition;
            closedChestModel.transform.localRotation = Quaternion.Euler(
                data.closedChestModelData.relativeRotation
            );
            closedChestModel.transform.localScale = data.closedChestModelData.relativeSize;
            closedChestModel.SetActive(true);

            openChestModel = await SetModel(data.opendedChestModelData.modelId);
            openChestModel.transform.localPosition = data.opendedChestModelData.relativePosition;
            openChestModel.transform.localRotation = Quaternion.Euler(
                data.opendedChestModelData.relativeRotation
            );
            openChestModel.transform.localScale = data.opendedChestModelData.relativeSize;
            openChestModel.SetActive(false);
        }

        // public methods for editor

        public async Task SetClosedModel(string modelId)
        {
            data.closedChestModelData ??= new ChestModelData();
            data.closedChestModelData.modelId = modelId;
            if (closedChestModel != null)
                Destroy(closedChestModel);
            closedChestModel = await SetModel(modelId);
            closedChestModel.SetActive(true);
        }

        public async Task SetOpenedModel(string modelId)
        {
            data.opendedChestModelData ??= new ChestModelData();
            data.opendedChestModelData.modelId = modelId;
            if (openChestModel != null)
                Destroy(openChestModel);
            openChestModel = await SetModel(modelId);
            openChestModel.SetActive(false);
        }

        public GameObject GetClosedModel() => closedChestModel;

        public GameObject GetOpenedModel() => openChestModel;

        public void ToggleChestModels(bool isOpen)
        {
            if (openChestModel != null)
                openChestModel.SetActive(isOpen);
            if (closedChestModel != null)
                closedChestModel.SetActive(!isOpen);
        }

        public void SyncColliderToActiveModel()
        {
            GameObject activeModel =
                closedChestModel != null && closedChestModel.activeSelf
                    ? closedChestModel
                    : openChestModel;

            if (activeModel == null)
                return;

            var parentCollider = GetComponent<BoxCollider>();
            if (parentCollider == null)
                return;

            // use mesh bounds since BoxCollider was already removed from the model
            var renderers = activeModel.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return;

            Bounds combined = renderers[0].bounds;
            foreach (var r in renderers)
                combined.Encapsulate(r.bounds);

            parentCollider.center = transform.InverseTransformPoint(combined.center);
            Vector3 parentScale = transform.lossyScale;
            parentCollider.size = new Vector3(
                combined.size.x / parentScale.x,
                combined.size.y / parentScale.y,
                combined.size.z / parentScale.z
            );
        }

        // Saving and Loading

        public override void SaveData(ref ZoneData worldData)
        {
            data.position = transform.position;
            data.rotation = transform.rotation.eulerAngles;
            data.size = transform.localScale;

            data.closedChestModelData.relativePosition = closedChestModel.transform.localPosition;
            data.closedChestModelData.relativeRotation = closedChestModel
                .transform
                .localEulerAngles;
            data.closedChestModelData.relativeSize = closedChestModel.transform.localScale;

            data.opendedChestModelData.relativePosition = openChestModel.transform.localPosition;
            data.opendedChestModelData.relativeRotation = openChestModel.transform.localEulerAngles;
            data.opendedChestModelData.relativeSize = openChestModel.transform.localScale;

            worldData.chestPlacements.Add(data);
        }

        public override void LoadData(ChestData data)
        {
            this.data = data;
            gameObject.name = data.name;
            transform.position = data.position;
            transform.localScale = data.size;
            transform.rotation = Quaternion.Euler(data.rotation);
        }

        // Helper methods

        private async Task<GameObject> SetModel(string modelId)
        {
            GameObject model = await placeablesLibrary.GetObject(
                PlaceableObjectCategories.Structure,
                modelId
            );
            model.transform.SetParent(transform, false);

            var modelCollider = model.GetComponent<BoxCollider>();
            var parentCollider = GetComponent<BoxCollider>();

            if (modelCollider != null && parentCollider != null)
            {
                Vector3 worldCenter = model.transform.TransformPoint(modelCollider.center);
                parentCollider.center = transform.InverseTransformPoint(worldCenter);

                Vector3 modelScale = model.transform.lossyScale;
                Vector3 parentScale = transform.lossyScale;
                parentCollider.size = new Vector3(
                    modelCollider.size.x * modelScale.x / parentScale.x,
                    modelCollider.size.y * modelScale.y / parentScale.y,
                    modelCollider.size.z * modelScale.z / parentScale.z
                );
            }

            Destroy(model.GetComponent<StructureObject>());
            Destroy(model.GetComponent<BoxCollider>());

            return model;
        }
    }
}
