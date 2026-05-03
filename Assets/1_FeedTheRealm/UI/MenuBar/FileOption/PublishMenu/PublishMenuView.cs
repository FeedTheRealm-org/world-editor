using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.MenuBar.FileOption.PublishMenu
{
    public class PublishMenuView
    {
        private Button publishButton;
        private Button loginButton;
        private Button closeButton;
        private Label worldNameLabel;
        private Toggle publishCreatablesToggle;
        private Toggle publishWorldDataToggle;
        private VisualElement zoneGroup;

        private Button allZonesButton;
        private readonly Dictionary<int, Button> zoneButtons = new();

        public event Action OnPublishClicked;
        public event Action OnCloseClicked;
        public event Action OnLoginClicked;
        public event Action OnAllZonesClicked;
        public event Action<int> OnZoneClicked;

        public bool PublishCreatables => publishCreatablesToggle?.value ?? false;
        public bool PublishWorldData => publishWorldDataToggle?.value ?? false;

        public void Initialize(VisualElement root)
        {
            publishButton = root.Q<Button>("Publish");
            loginButton = root.Q<Button>("Login");
            closeButton = root.Q<Button>("Close");
            worldNameLabel = root.Q<Label>("WorldName");
            publishCreatablesToggle = root.Q<Toggle>("PublishCreatables");
            publishWorldDataToggle = root.Q<Toggle>("PublishWorldData");
            zoneGroup = root.Q<VisualElement>("ZoneGroup");

            publishButton.clicked += () => OnPublishClicked?.Invoke();
            closeButton.clicked += () => OnCloseClicked?.Invoke();
            loginButton.clicked += () => OnLoginClicked?.Invoke();
        }

        public void SetWorldName(string name)
        {
            if (worldNameLabel != null)
                worldNameLabel.text = name ?? "No world loaded";
        }

        public void SetPublishWorldDataToggle(bool value, bool enabled)
        {
            if (publishWorldDataToggle == null)
                return;
            publishWorldDataToggle.value = value;
            publishWorldDataToggle.SetEnabled(enabled);
        }

        public void SetPublishCreatablesToggle(bool value)
        {
            if (publishCreatablesToggle != null)
                publishCreatablesToggle.value = value;
        }

        public void SetPublishButtonEnabled(bool enabled)
        {
            if (publishButton != null)
                publishButton.SetEnabled(enabled);
        }

        public void ConfigureZoneScrollView()
        {
            if (zoneGroup is ScrollView scrollView)
            {
                scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
                scrollView.contentContainer.style.flexDirection = FlexDirection.Column;
                scrollView.contentContainer.style.width = Length.Percent(100);
            }
        }

        public void ClearZones()
        {
            zoneGroup?.Clear();
            zoneButtons.Clear();
            allZonesButton = null;
        }

        public void AddAllZonesButton()
        {
            allZonesButton = CreateZoneButton("All Zones", true);
            allZonesButton.clicked += () => OnAllZonesClicked?.Invoke();
            zoneGroup.Add(allZonesButton);
        }

        public void AddZoneButton(int zoneId)
        {
            var button = CreateZoneButton($"Zone {zoneId}", false);
            button.clicked += () => OnZoneClicked?.Invoke(zoneId);
            zoneButtons[zoneId] = button;
            zoneGroup.Add(button);
        }

        public void SetAllZonesSelected(bool selected)
        {
            SetButtonSelected(allZonesButton, selected);
        }

        public void SetZoneSelected(int zoneId, bool selected)
        {
            if (zoneButtons.TryGetValue(zoneId, out var button))
                SetButtonSelected(button, selected);
        }

        public void DeselectAllZoneButtons()
        {
            foreach (var button in zoneButtons.Values)
                SetButtonSelected(button, false);
        }

        private Button CreateZoneButton(string text, bool selected)
        {
            var button = new Button { text = text };
            button.style.marginBottom = 2;
            button.style.marginLeft = 2;
            button.style.marginRight = 2;
            button.style.flexShrink = 0;
            button.style.alignSelf = Align.Stretch;
            button.style.width = Length.Percent(100);
            button.style.marginBottom = 4;
            button.style.backgroundColor = selected
                ? new StyleColor(new Color(0.2f, 0.6f, 0.2f))
                : new StyleColor(Color.black);
            button.style.color = new StyleColor(Color.white);
            button.style.borderTopLeftRadius = 6;
            button.style.borderTopRightRadius = 6;
            button.style.borderBottomLeftRadius = 6;
            button.style.borderBottomRightRadius = 6;
            return button;
        }

        private void SetButtonSelected(Button button, bool selected)
        {
            if (button == null)
                return;
            button.style.backgroundColor = selected
                ? new StyleColor(new Color(0.2f, 0.6f, 0.2f))
                : new StyleColor(Color.black);
        }
    }
}
