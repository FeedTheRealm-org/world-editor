using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Gameplay.WorldLoader;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

public class ZoneOptionController : MonoBehaviour
{
    [SerializeField]
    private string CreateNewOptionDisplay = "Create new zone";

    [Inject]
    private DataPersistenceManager dataPersistenceManager;

    [Inject]
    private WorldSelector worldSelector;

    [Inject]
    private ZoneLoader zoneLoader;

    [Inject]
    private RefreshZonesEvent refreshZonesEvent;
    private DropdownField zoneDropdown;
    private VisualElement root;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        zoneDropdown = root.Q<DropdownField>("Zone");

        if (zoneDropdown == null)
        {
            Debug.LogError("[ZoneOptionController] Zone dropdown not found.");
            return;
        }

        RenderDropdown();
        BindEvents();

        refreshZonesEvent.OnRaised += RenderDropdown;
    }

    private void RenderDropdown()
    {
        var zones = dataPersistenceManager.ListZones(worldSelector.selectedWorld);

        zoneDropdown.choices = zones.ConvertAll(z => z.ToString());
        zoneDropdown.choices.Add(CreateNewOptionDisplay);
        zoneDropdown.value = worldSelector.selectedZoneId.ToString();
    }

    private void BindEvents()
    {
        zoneDropdown.RegisterValueChangedCallback(async evt =>
        {
            await OnZoneSelected(evt.newValue);
        });
    }

    private async UniTask OnZoneSelected(string selectedValue)
    {
        if (selectedValue == CreateNewOptionDisplay)
            await CreateNewZone();
        else
            await LoadZone(selectedValue);
        zoneDropdown.value = worldSelector.selectedZoneId.ToString();
    }

    private async UniTask LoadZone(string selectedValue)
    {
        int selectedZone = int.Parse(selectedValue);
        if (selectedZone == worldSelector.selectedZoneId)
            return;
        worldSelector.selectedZoneId = selectedZone;
        //dataPersistenceManager.SaveZone(worldSelector.selectedWorld, worldSelector.selectedZoneId);
        Debug.Log(
            $"[ZoneOptionController] {worldSelector.selectedWorld} | Zone {selectedValue} selected."
        );
        await zoneLoader.Load();
    }

    /// <summary>
    /// Creates a new zone with the next available ID, loads it, saves it, and updates the dropdown.
    /// </summary>
    private async UniTask CreateNewZone()
    {
        if (string.IsNullOrEmpty(worldSelector.selectedWorld))
        {
            ToastNotification.Show(
                "Please save your world first before creating a new zone.",
                "error",
                Color.red
            );
            return;
        }
        int newZoneId = dataPersistenceManager.GetNextZoneId(worldSelector.selectedWorld);
        worldSelector.selectedZoneId = newZoneId;
        await zoneLoader.Load();
        dataPersistenceManager.SaveZone(worldSelector.selectedWorld, worldSelector.selectedZoneId);
        ToastNotification.Show($"Zone {newZoneId} created!", "success", Color.green);
        RenderDropdown();
    }
}
