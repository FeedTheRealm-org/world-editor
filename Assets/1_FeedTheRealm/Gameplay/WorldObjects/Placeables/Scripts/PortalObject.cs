using System.Linq;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FTRShared.Runtime.Models;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Gameplay.WorldObjects
{
    public class PortalObject : Placeable<PortalPlacementData>
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [Inject]
        private WorldSelector worldSelector;

        public override PlaceableObjectCategories Category => PlaceableObjectCategories.Portal;

        public override void SaveData(ref ZoneData worldData)
        {
            data.position = gameObject.transform.position;
            data.radius = transform.localScale.x;
            worldData.portalPlacements.Add(data);
        }

        private void Start()
        {
            // If the portal already exists in the creatables manager, we don't need to do anything,
            // since it means we are loading an existing portal from the zone data,
            if (creatablesManager.GetAll<Portal>().Any(p => p.Id == data.id))
                return;

            // If it doesn't exist, it means we are placing a new portal,
            // so we create a new Portal creatable and add it to the manager.
            Portal newPortal = new(worldSelector.selectedZoneId);
            creatablesManager.Add(newPortal);

            // we link the placeable portal with the creatable portal
            data.id = newPortal.Id;
        }

        public override void DeletePlaceable()
        {
            // When deleting a portal, we also need to delete the corresponding creatable portal,
            // since it won't be used anymore and we want to keep the creatables manager clean.
            creatablesManager.Delete<Portal>(data.id);
            foreach (var portal in creatablesManager.GetAll<Portal>())
            {
                if (portal.data.targetPortalId == data.id)
                {
                    portal.data.targetPortalId = null;
                }
            }
            base.DeletePlaceable();
        }

        public override void LoadData(PortalPlacementData data)
        {
            this.data = data;
            gameObject.name = data.name;
            gameObject.transform.position = data.position;
            gameObject.transform.localScale = new Vector3(
                data.radius,
                transform.localScale.y,
                data.radius
            );
        }
    }
}
