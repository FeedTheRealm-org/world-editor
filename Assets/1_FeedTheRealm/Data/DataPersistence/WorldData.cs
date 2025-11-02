using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class WorldData {
    [SerializeField]
    public string worldName;

    [SerializeField]
    public List<PlacementData> objectPlacementData;

}
