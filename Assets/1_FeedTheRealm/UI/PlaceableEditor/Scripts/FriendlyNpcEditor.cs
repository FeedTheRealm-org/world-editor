using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Core.WorldObjects.NPCs;
using FeedTheRealm.Gameplay.Library.CreatorObjectLibrary;
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
        // [Inject] private CreatorObjectLibrarySO creatorObjectLibrary;
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
                var selected = GetNpcs().FirstOrDefault(npc => npc.DisplayName == e.newValue);
                if (selected != null)
                    target.data.NpcId = selected.ObjectId;
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

            var npcs = GetNpcs();
            npcDropdown.choices = npcs.Select(n => n.DisplayName).ToList();

            if (!string.IsNullOrEmpty(target.data.NpcId))
            {
                var current = npcs.FirstOrDefault(n => n.ObjectId == target.data.NpcId);
                if (current != null)
                    npcDropdown.SetValueWithoutNotify(current.DisplayName);
            }
        }

        private List<GenericNPC> GetNpcs() => new List<GenericNPC>();
        // creatorObjectLibrary
        //     .GetCreatables(CreatorObjectCategories.NPC)
        //     .Cast<GenericNPC>()
        //     .ToList();
    }
}
