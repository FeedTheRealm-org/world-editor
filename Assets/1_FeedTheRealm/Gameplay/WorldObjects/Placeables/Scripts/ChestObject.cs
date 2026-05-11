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
        private StructureLibrary structureLibrary;

        public override PlaceableObjectCategories Category => PlaceableObjectCategories.Chest;

        private GameObject openChestModel;
        private GameObject closedChestModel;

        private async void Start()
        {
            bool isNew = data == null || string.IsNullOrEmpty(data.id);

            if (isNew)
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

            (closedChestModel, bool isClosedModelDefault) = await ApplyModel(
                data.closedChestModelData.modelId,
                data.closedChestModelData,
                isNew,
                active: true
            );
            data.closedChestModelData.isDefault = isClosedModelDefault;

            (openChestModel, bool isOpenModelDefault) = await ApplyModel(
                data.opendedChestModelData.modelId,
                data.opendedChestModelData,
                isNew,
                active: false
            );
            data.opendedChestModelData.isDefault = isOpenModelDefault;
        }

        // public methods for editor

        public async Task SetClosedModel(string modelId)
        {
            data.closedChestModelData ??= new ChestModelData();
            data.closedChestModelData.modelId = modelId;
            if (closedChestModel != null)
                Destroy(closedChestModel);
            (closedChestModel, bool isDefault) = await SetModel(modelId);
            data.closedChestModelData.isDefault = isDefault;
            closedChestModel.SetActive(true);
        }

        public async Task SetOpenedModel(string modelId)
        {
            data.opendedChestModelData ??= new ChestModelData();
            data.opendedChestModelData.modelId = modelId;
            if (openChestModel != null)
                Destroy(openChestModel);
            (openChestModel, bool isDefault) = await SetModel(modelId);
            data.opendedChestModelData.isDefault = isDefault;
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

        private async Task<(GameObject model, bool isDefault)> ApplyModel(
            string modelId,
            ChestModelData modelData,
            bool isNew,
            bool active
        )
        {
            var (model, isDefault) = await SetModel(modelId);

            if (isNew)
            {
                modelData.relativePosition = model.transform.localPosition;
                modelData.relativeRotation = model.transform.localEulerAngles;
                modelData.relativeSize = model.transform.localScale;
            }

            model.transform.localPosition = modelData.relativePosition;
            model.transform.localRotation = Quaternion.Euler(modelData.relativeRotation);
            model.transform.localScale = modelData.relativeSize;
            model.SetActive(active);

            return (model, isDefault);
        }

        private async Task<(GameObject model, bool isDefault)> SetModel(string modelId)
        {
            GameObject model = await structureLibrary.GetItem(modelId);
            bool isDefault = true;
            model.transform.SetParent(transform, false);
            var modelCollider = model.GetComponent<BoxCollider>();
            var parentCollider = GetComponent<BoxCollider>();
            Vector3 worldCenter = model.transform.TransformPoint(modelCollider.center);
            parentCollider.center = transform.InverseTransformPoint(worldCenter);

            Vector3 modelScale = model.transform.lossyScale;
            Vector3 parentScale = transform.lossyScale;
            parentCollider.size = new Vector3(
                modelCollider.size.x * modelScale.x / parentScale.x,
                modelCollider.size.y * modelScale.y / parentScale.y,
                modelCollider.size.z * modelScale.z / parentScale.z
            );
            try
            {
                isDefault = model.GetComponent<StructureObject>().data.isDefault;
                Destroy(model.GetComponent<StructureObject>());
                Destroy(model.GetComponent<BoxCollider>());
            }
            catch (Exception e)
            {
                ToastNotification.Show(
                    $"Chest could not load model, make sure to set a default model in the config",
                    "error",
                    Color.red
                );
                Debug.LogError($"Failed to apply model '{modelId}' to chest: {e.Message}");
            }
            Debug.Log($"Model '{model != null}' applied to chest. Is default: {isDefault}");
            return (model, isDefault);
        }
    }
}
