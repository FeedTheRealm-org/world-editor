using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.Gameplay.WorldObjects;
using FTR.UI;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.PlaceableEditor
{
    [RequireComponent(typeof(UIDocument))]
    public class PortalEditor : MenuController, IEditable
    {
        [Inject]
        private CreatablesManager creatablesManager;

        [Inject]
        private DataPersistenceManager dataPersistenceManager;

        [Inject]
        private WorldSelector worldSelector;

        private TextField portalNameField;
        private DropdownField portalDestinationDropdown;
        private DropdownField zoneDropdown;
        private Slider radiusSlider;
        private Button closeButton;
        private Vector3Field positionField;

        private PortalObject target;
        private Portal targetPortal;
        private List<Portal> allPortals;
        private PositionGizmo positionGizmo;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            portalNameField = root.Q<TextField>("PortalName");
            portalDestinationDropdown = root.Q<DropdownField>("PortalDestination");
            zoneDropdown = root.Q<DropdownField>("Zone");
            radiusSlider = root.Q<Slider>("PortalRadius");
            closeButton = root.Q<Button>("Close");
            positionField = root.Q<Vector3Field>("Position");

            positionField.RegisterValueChangedCallback(e => target.transform.position = e.newValue);

            closeButton.clicked += CloseMenu;
            portalNameField.RegisterCallback<FocusInEvent>(_ => inputReader.ToggleInput(false));
            portalNameField.RegisterCallback<FocusOutEvent>(_ => inputReader.ToggleInput(true));
        }

        public void Edit(GameObject placeable)
        {
            target = placeable.GetComponent<PortalObject>();
            if (target == null)
            {
                Debug.LogError($"PortalEditor: {placeable.name} has no PortalObject component.");
                CloseMenu();
                return;
            }

            targetPortal = creatablesManager
                .GetAll<Portal>()
                .FirstOrDefault(p => p.Id == target.data.id);

            if (targetPortal == null)
            {
                Debug.LogError($"PortalEditor: No Portal creatable found for id {target.data.id}");
                CloseMenu();
                return;
            }

            PopulateFields();
            BindEvents();
            SetupGizmo();
        }

        private void PopulateFields()
        {
            portalNameField.SetValueWithoutNotify(targetPortal.data.name);
            radiusSlider.SetValueWithoutNotify(target.data.radius);
            positionField.SetValueWithoutNotify(target.transform.position);

            PopulateZoneDropdown();
            PopulateDestinationDropdown();
        }

        private void PopulateZoneDropdown()
        {
            var zones = dataPersistenceManager.ListZones(worldSelector.selectedWorld);

            var choices = new List<string> { "All Zones" };
            choices.AddRange(zones.Select(z => $"Zone {z}"));
            zoneDropdown.choices = choices;
            zoneDropdown.SetValueWithoutNotify("All Zones");
        }

        private void PopulateDestinationDropdown()
        {
            string selectedZone = zoneDropdown.value;

            allPortals = creatablesManager
                .GetAll<Portal>()
                .Where(p => p.Id != targetPortal.Id)
                .Where(p => selectedZone == "All Zones" || $"Zone {p.data.zoneId}" == selectedZone)
                .ToList();

            var choices = new List<string> { "None" };
            choices.AddRange(allPortals.Select(p => p.data.name));
            portalDestinationDropdown.choices = choices;

            if (!string.IsNullOrEmpty(targetPortal.data.targetPortalId))
            {
                var current = allPortals.FirstOrDefault(p =>
                    p.Id == targetPortal.data.targetPortalId
                );
                portalDestinationDropdown.SetValueWithoutNotify(
                    current != null ? current.data.name : "None"
                );
            }
            else
                portalDestinationDropdown.SetValueWithoutNotify("None");
        }

        private void BindEvents()
        {
            portalNameField.RegisterValueChangedCallback(evt =>
            {
                targetPortal.data.name = evt.newValue;
                target.data.name = evt.newValue;
                target.gameObject.name = $"Portal-{evt.newValue}";
            });

            radiusSlider.RegisterValueChangedCallback(evt =>
            {
                target.data.radius = evt.newValue;
                target.transform.localScale = new Vector3(
                    evt.newValue,
                    target.transform.localScale.y,
                    evt.newValue
                );
            });

            zoneDropdown.RegisterValueChangedCallback(evt =>
            {
                PopulateDestinationDropdown();
            });

            portalDestinationDropdown.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == "None")
                {
                    targetPortal.data.targetPortalId = null;
                    return;
                }

                var selected = allPortals.FirstOrDefault(p => p.data.name == evt.newValue);
                if (selected != null)
                    targetPortal.data.targetPortalId = selected.Id;
            });
        }

        private void SetupGizmo()
        {
            positionGizmo = GetComponentInChildren<PositionGizmo>(includeInactive: true);
            if (positionGizmo == null)
                return;

            positionGizmo.Initialize(target.transform, Camera.main);
            positionGizmo.OnPositionChanged += OnGizmoMoved;
            positionGizmo.gameObject.SetActive(true);
        }

        private void OnGizmoMoved(Vector3 newPosition)
        {
            positionField.SetValueWithoutNotify(newPosition);
        }

        public override void CloseMenu()
        {
            base.CloseMenu();
            positionGizmo.OnPositionChanged -= OnGizmoMoved;
        }
    }
}
