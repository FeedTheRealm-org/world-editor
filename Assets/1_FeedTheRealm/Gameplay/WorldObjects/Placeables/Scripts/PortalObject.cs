using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldObjects
{
    public class PortalObject : Placeable<PortalPlacementData>
    {
        public PortalPlacementData data;

        public override PlaceableObjectCategories Category => PlaceableObjectCategories.Portal;

        public override void SaveData(ref ZoneData worldData)
        {
            worldData.portalPlacements.Add(data);
        }

        public override void LoadData(PortalPlacementData data)
        {
            this.data = data;
            gameObject.name = $"Portal-{data.id}";
            gameObject.transform.position = data.position;
            gameObject.transform.localScale = new Vector3(
                data.Radius,
                transform.localScale.y,
                data.Radius
            );
        }
    }
}
