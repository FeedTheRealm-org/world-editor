using UnityEngine;

public static class WorldLayers
{
    private static string worldObjectLayer = "WorldObject";
    public static readonly LayerMask WorldObjectLayerMask = LayerMask.GetMask(worldObjectLayer);
    public static readonly int WorldObjectLayer = LayerMask.NameToLayer(worldObjectLayer);
}
