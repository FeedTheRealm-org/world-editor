using System;
using System.Linq;
using Builders;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ConsumableItemCreatorMenuController : ItemCreatorMenuController<ConsumableItem>
{
    private DropdownField effectTypeInput;
    private IntegerField valueInput;
    private FloatField durationInput;
    private FloatField cooldownInput;
    private IntegerField maxStackInput;

    private ItemDataBuilder itemDataBuilder = new ItemDataBuilder();

    protected override CreatorObjectCategories Category => CreatorObjectCategories.ConsumableItem;

    protected override void InitializeSpecificFields(VisualElement root)
    {
        effectTypeInput = root.Q<DropdownField>("EffectTypeField");
        if (effectTypeInput == null)
            logger.Log("Effect type dropdown field not found in UI", this, Logging.LogType.Error);
        valueInput = root.Q<IntegerField>("EffectValueField");
        if (valueInput == null)
            logger.Log("Effect value input field not found in UI", this, Logging.LogType.Error);
        durationInput = root.Q<FloatField>("EffectDurationField");
        if (durationInput == null)
            logger.Log("Effect duration input field not found in UI", this, Logging.LogType.Error);
        cooldownInput = root.Q<FloatField>("EffectCooldownField");
        if (cooldownInput == null)
            logger.Log("Effect cooldown input field not found in UI", this, Logging.LogType.Error);
        maxStackInput = root.Q<IntegerField>("MaxStackField");
        if (maxStackInput == null)
            logger.Log("Max stack input field not found in UI", this, Logging.LogType.Error);

        if (effectTypeInput != null)
        {
            effectTypeInput.choices = Enum.GetNames(typeof(EffectType)).ToList();
        }
    }

    protected override void PopulateFields()
    {
        nameInput.value = currentItem.name;
        descriptionInput.value = currentItem.description ?? "";
        valueInput.value = currentItem.value;
        durationInput.value = currentItem.duration;
        cooldownInput.value = currentItem.cooldown;
        maxStackInput.value = currentItem.maxStack;
        effectTypeInput.value = currentItem.effectType.ToString();
        LoadExistingSprite(currentItem.spriteFile);
    }

    protected override void OnSaveClicked()
    {
        string savedSpritePath = SaveSpriteIfNeeded();

        if (currentItem == null)
        {
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

            currentItem = new ConsumableItem(consumableItemData);
            creatorObjectLibrary.AddCreatable(Category, currentItem);
            logger.Log(
                $"Created new consumable item: {currentItem.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }
        else
        {
            currentItem.name = nameInput.value;
            currentItem.description = descriptionInput.value;
            currentItem.value = valueInput.value;
            currentItem.duration = durationInput.value;
            currentItem.cooldown = cooldownInput.value;
            currentItem.maxStack = maxStackInput.value;
            currentItem.spriteFile = savedSpritePath ?? currentItem.spriteFile;
            currentItem.effectType = (EffectType)
                Enum.Parse(typeof(EffectType), effectTypeInput.value);
            logger.Log(
                $"Updated consumable item: {currentItem.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }
        ReturnToItemsMenu();
    }
}
