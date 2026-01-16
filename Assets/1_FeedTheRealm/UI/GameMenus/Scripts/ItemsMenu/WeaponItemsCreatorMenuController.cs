using System;
using System.Linq;
using Builders;
using Models;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class WeaponItemCreatorMenuController : ItemCreatorMenuController<WeaponItem>
{
    private DropdownField weaponTypeInput;
    private IntegerField damageInput;
    private FloatField attackSpeedInput;
    private FloatField rangeInput;
    private IntegerField ammoInput;

    private ItemDataBuilder itemDataBuilder = new ItemDataBuilder();

    protected override CreatorObjectCategories Category => CreatorObjectCategories.WeaponItem;

    protected override void InitializeSpecificFields(VisualElement root)
    {
        weaponTypeInput = root.Q<DropdownField>("WeaponTypeField");
        if (weaponTypeInput == null)
            logger.Log("Weapon type dropdown field not found in UI", this, Logging.LogType.Error);
        damageInput = root.Q<IntegerField>("DamageField");
        if (damageInput == null)
            logger.Log("Damage input field not found in UI", this, Logging.LogType.Error);
        attackSpeedInput = root.Q<FloatField>("AttackSpeedField");
        if (attackSpeedInput == null)
            logger.Log("Attack speed input field not found in UI", this, Logging.LogType.Error);
        rangeInput = root.Q<FloatField>("RangeField");
        if (rangeInput == null)
            logger.Log("Range input field not found in UI", this, Logging.LogType.Error);
        ammoInput = root.Q<IntegerField>("AmmoField");
        if (ammoInput == null)
            logger.Log("Ammo input field not found in UI", this, Logging.LogType.Error);

        if (weaponTypeInput != null)
        {
            weaponTypeInput.choices = Enum.GetNames(typeof(WeaponType)).ToList();
        }
    }

    protected override void PopulateFields()
    {
        nameInput.value = currentItem.name;
        descriptionInput.value = currentItem.description ?? "";
        weaponTypeInput.value = currentItem.weaponType.ToString();
        damageInput.value = currentItem.damage;
        attackSpeedInput.value = currentItem.attackSpeed;
        rangeInput.value = currentItem.range;
        ammoInput.value = currentItem.ammo;
        LoadExistingSprite(currentItem.spriteFile);
    }

    protected override void OnSaveClicked()
    {
        string savedSpritePath = SaveSpriteIfNeeded();

        if (string.IsNullOrEmpty(nameInput.value))
        {
            logger?.Log("Weapon item name is required", this, Logging.LogType.Warning);
            ToastNotification.Show("Weapon item name is required", "error", Color.red);
            return;
        }

        if (string.IsNullOrEmpty(savedSpritePath))
        {
            logger?.Log("Weapon item sprite is required", this, Logging.LogType.Warning);
            ToastNotification.Show("Weapon item sprite is required", "error", Color.red);
            return;
        }

        if (string.IsNullOrEmpty(weaponTypeInput.value))
        {
            logger?.Log("Weapon type is required", this, Logging.LogType.Warning);
            ToastNotification.Show("Weapon type is required", "error", Color.red);
            return;
        }

        if (currentItem == null)
        {
            var weaponItemData = itemDataBuilder
                .SetItemData(
                    null,
                    nameInput.value,
                    descriptionInput.value ?? "",
                    savedSpritePath ?? ""
                )
                .BuildWeaponItem(
                    (WeaponType)Enum.Parse(typeof(WeaponType), weaponTypeInput.value),
                    damageInput.value,
                    attackSpeedInput.value,
                    rangeInput.value,
                    ammoInput.value
                );

            currentItem = new WeaponItem(weaponItemData);
            creatorObjectLibrary.AddCreatable(Category, currentItem);
            logger.Log(
                $"Created new weapon item: {currentItem.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }
        else
        {
            currentItem.name = nameInput.value;
            currentItem.description = descriptionInput.value;
            currentItem.weaponType = (WeaponType)
                Enum.Parse(typeof(WeaponType), weaponTypeInput.value);
            currentItem.damage = damageInput.value;
            currentItem.attackSpeed = attackSpeedInput.value;
            currentItem.range = rangeInput.value;
            currentItem.ammo = ammoInput.value;
            currentItem.spriteFile = savedSpritePath ?? currentItem.spriteFile;
            logger.Log(
                $"Updated weapon item: {currentItem.DisplayName}",
                this,
                Logging.LogType.Info
            );
        }

        ToastNotification.Show("Weapon item saved successfully", "success", Color.green);

        ReturnToItemsMenu();
    }
}
