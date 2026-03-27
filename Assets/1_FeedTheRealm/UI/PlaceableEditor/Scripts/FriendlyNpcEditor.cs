using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.Gameplay.WorldObjects;
using FeedTheRealm.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace FeedTheRealm.UI.PlaceableEditor
{
    [RequireComponent(typeof(UIDocument))]
    public class FriendlyNpcSpawnerEditor : MenuController, IEditable
    {
        [Inject]
        private CreatablesManager CreatablesManager;
        private Slider radiusSlider;
        private DropdownField npcDropdown;
        private Button closeButton;
        private FriendlyNpcSpawnerObject target;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            radiusSlider = root.Q<Slider>("SpawnerRadius");
            npcDropdown = root.Q<DropdownField>("NPCDropdown");
            closeButton = root.Q<Button>("Close");

            radiusSlider.RegisterValueChangedCallback(e =>
            {
                target.data.Radius = e.newValue;
                target.transform.localScale = new Vector3(
                    e.newValue,
                    target.transform.localScale.y,
                    e.newValue
                );
            });

            npcDropdown.RegisterValueChangedCallback(e =>
            {
                var selected = CreatablesManager
                    .GetAll<FriendlyNpc>()
                    .FirstOrDefault(npc => npc.data.name == e.newValue);
                if (selected != null)
                    target.data.NpcId = selected.Id;
            });

            closeButton.clicked += CloseMenu;
        }

        public void Edit(GameObject placeable)
        {
            target = placeable.GetComponent<FriendlyNpcSpawnerObject>();
            if (target == null)
            {
                Debug.LogError(
                    $"FriendlyNpcSpawnerEditor: {placeable.name} has no FriendlyNpcSpawnerObject component."
                );
                CloseMenu();
                return;
            }

            PopulateFields();
        }

        private void PopulateFields()
        {
            radiusSlider.SetValueWithoutNotify(target.data.Radius);

            var npcs = CreatablesManager.GetAll<FriendlyNpc>();
            npcDropdown.choices = npcs.Select(n => n.data.name).ToList();

            if (!string.IsNullOrEmpty(target.data.NpcId))
            {
                var current = npcs.FirstOrDefault(n => n.Id == target.data.NpcId);
                if (current != null)
                    npcDropdown.SetValueWithoutNotify(current.data.name);
            }
        }
    }
}
