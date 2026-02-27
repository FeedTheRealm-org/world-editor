using UnityEngine;

[CreateAssetMenu(
    fileName = "WorldPrefabProvider",
    menuName = "Scriptable Objects/WorldPrefabProvider"
)]
public class WorldPrefabProvider : ScriptableObject
{
    public GameObject playerPrefab;
    public GameObject worldPrefab;
    public GameObject worldEditorPrefab;
}
