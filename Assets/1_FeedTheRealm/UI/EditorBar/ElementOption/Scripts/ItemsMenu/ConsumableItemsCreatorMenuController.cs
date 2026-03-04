using System;
using System.Linq;
using Builders;
using FeedTheRealm.Core.WorldObjects.Items;
using FeedTheRealm.UI.EditorBar.ElementOption.Base;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.EditorBar.ElementOption.ItemsMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class ConsumableItemCreatorMenuController : ItemCreatorMenuController<ConsumableItem>
    {
        private DropdownField effectTypeInput;
        private IntegerField valueInput;
        private FloatField durationInput;
        private FloatField cooldownInput;
        private IntegerField maxStackInput;

        private ItemDataBuilder itemDataBuilder = new ItemDataBuilder();

        protected override CreatorObjectCategories Category =>
            CreatorObjectCategories.ConsumableItem;
        protected override string ObjectTypeName => "Consumable Item";

        protected override void InitializeItemSpecificFields(VisualElement root)
        {
            effectTypeInput = root.Q<DropdownField>("EffectTypeField");
            LogIfNull(effectTypeInput, "Effect type dropdown field");

            valueInput = root.Q<IntegerField>("EffectValueField");
            LogIfNull(valueInput, "Effect value input field");

            durationInput = root.Q<FloatField>("EffectDurationField");
            LogIfNull(durationInput, "Effect duration input field");

            cooldownInput = root.Q<FloatField>("EffectCooldownField");
            LogIfNull(cooldownInput, "Effect cooldown input field");

            maxStackInput = root.Q<IntegerField>("MaxStackField");
            LogIfNull(maxStackInput, "Max stack input field");

            if (effectTypeInput != null)
            {
                effectTypeInput.choices = Enum.GetNames(typeof(EffectType)).ToList();
            }
        }

        protected override void PopulateFields()
        {
            nameInput.value = currentObject.name;
            descriptionInput.value = currentObject.description ?? "";
            valueInput.value = currentObject.value;
            durationInput.value = currentObject.duration;
            cooldownInput.value = currentObject.cooldown;
            maxStackInput.value = currentObject.maxStack;
            effectTypeInput.value = currentObject.effectType.ToString();
            LoadExistingSprite(currentObject.spriteFile);
        }

        protected override bool ValidateSpecificFields()
        {
            if (string.IsNullOrEmpty(effectTypeInput?.value))
            {
                ShowValidationError("Consumable item effect type is required");
                return false;
            }
            return true;
        }

        protected override void CreateNewObject()
        {
            string savedSpritePath = SaveSpriteIfNeeded();

            var consumableItemData = itemDataBuilder
                .SetItemData(
                    null,
                    nameInput.value,
                    descriptionInput.value ?? "",
                    savedSpritePath ?? ""
                )
                .BuildConsumableItem(
                    (EffectType)Enum.Parse(typeof(EffectType), effectTypeInput.value),
                    valueInput.value,
                    durationInput.value,
                    cooldownInput.value,
                    maxStackInput.value
                );

            currentObject = new ConsumableItem(consumableItemData);
            creatorObjectLibrary.AddCreatable(Category, currentObject);
            logger?.Log(
                $"Created new consumable item: {currentObject.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }

        protected override void UpdateExistingObject()
        {
            string savedSpritePath = SaveSpriteIfNeeded();

            currentObject.name = nameInput.value;
            currentObject.description = descriptionInput.value;
            currentObject.value = valueInput.value;
            currentObject.duration = durationInput.value;
            currentObject.cooldown = cooldownInput.value;
            currentObject.maxStack = maxStackInput.value;
            currentObject.spriteFile = savedSpritePath ?? currentObject.spriteFile;
            currentObject.effectType = (EffectType)
                Enum.Parse(typeof(EffectType), effectTypeInput.value);
            logger?.Log(
                $"Updated consumable item: {currentObject.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }
    }
}
