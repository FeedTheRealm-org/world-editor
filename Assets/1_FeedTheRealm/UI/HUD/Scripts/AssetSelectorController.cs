using System.Collections.Generic;
using UnityEngine;

public class AssetSelectorController : MonoBehaviour
{
    [SerializeField] private List<GameObject> assetList;




    public List<GameObject> GetAssetList()
    {
        return assetList;
    }
}
