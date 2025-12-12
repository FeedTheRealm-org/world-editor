using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ConsumableItems", menuName = "Scriptable Objects/Persistence/ConsumableItems")]
public class ConsumableItems : ScriptableObject {
  [System.Serializable]
  public class ConsumableData {
    [SerializeField]
    public string name;
    [SerializeField]
    public string description;
    [SerializeField]

    public string effectType; // e.g., Heal, Buff, Damage, Speed, Mana, Custom
    [SerializeField]
    public int value; // magnitude of effect
    [SerializeField]
    public float duration; // seconds; 0 if instant
    [SerializeField]
    public float cooldown; // seconds
    [SerializeField]

    public int maxStack; // default 1
    [SerializeField]
    public Sprite sprite;
  }

  [SerializeField] private Logging.Logger logger;

  [Header("List of Consumable Items")]
  [SerializeField] private List<ConsumableData> consumableItems = new List<ConsumableData>();

  public void AddConsumableItem(ConsumableData item) {
    if (consumableItems == null) consumableItems = new List<ConsumableData>();
    logger.Log($"Adding consumable item: {item.name}", this, Logging.LogType.Info);
    consumableItems.Add(item);
  }

  public List<ConsumableData> GetAllConsumableItems() {
    if (consumableItems == null) return new List<ConsumableData>();
    return consumableItems;
  }
}
