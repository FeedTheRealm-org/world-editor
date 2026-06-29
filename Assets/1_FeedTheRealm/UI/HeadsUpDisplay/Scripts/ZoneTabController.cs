using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Gameplay.Inputs;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.HeadsUpDisplay
{
    public class ZoneTabController : MonoBehaviour
    {
        [Inject]
        private EnableInteractionsEvent enableInputEvent;

        [Inject]
        private Logging.Logger logger;

        private VisualElement zonePill;
        private Label zoneBadge;
        private VisualElement zonePanel;
        private ScrollView zoneList;
        private Button addZoneBtn;

        private List<int> zones = new();
        private int activeZone = -1;
        private bool panelOpen = false;

        void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            zonePill = root.Q<VisualElement>("ZonePill");
            zoneBadge = root.Q<Label>("ZoneBadge");
            zonePanel = root.Q<VisualElement>("ZonePanel");
            zoneList = root.Q<ScrollView>("ZoneList");
            addZoneBtn = root.Q<Button>("AddZone");

            zonePill.RegisterCallback<ClickEvent>(_ => TogglePanel());
            addZoneBtn.clicked += AddZone;

            zonePill.RegisterCallback<MouseEnterEvent>(_ => enableInputEvent.Raise(false));
            zonePill.RegisterCallback<MouseLeaveEvent>(_ => enableInputEvent.Raise(true));

            RefreshBadge();
        }

        private void TogglePanel()
        {
            panelOpen = !panelOpen;
            zonePanel.style.display = panelOpen ? DisplayStyle.Flex : DisplayStyle.None;
            zonePill.EnableInClassList("hud-tab--active", panelOpen);
        }

        private void AddZone()
        {
            int newZone = zones.Count + 1;
            zones.Add(newZone);

            if (activeZone == -1)
                SetActiveZone(newZone);

            RefreshZoneList();
            RefreshBadge();
        }

        private void SetActiveZone(int zone)
        {
            activeZone = zone;
            RefreshBadge();
            RefreshZoneList();
            logger.Log($"Active zone set to {zone}", this, Logging.LogType.Info);
        }

        private void RefreshBadge()
        {
            zoneBadge.text = activeZone == -1 ? "+" : activeZone.ToString();
        }

        private void RefreshZoneList()
        {
            zoneList.Clear();

            foreach (var zone in zones)
            {
                var capturedZone = zone;
                var item = new Label(zone.ToString());
                item.AddToClassList("hud-tab__zone-item");

                if (zone == activeZone)
                    item.AddToClassList("hud-tab__zone-item--active");

                item.RegisterCallback<ClickEvent>(_ =>
                {
                    SetActiveZone(capturedZone);
                    TogglePanel();
                });

                zoneList.Add(item);
            }
        }
    }
}
