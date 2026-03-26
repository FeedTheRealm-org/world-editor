using System;
using System.Linq;
using Builders;
using FeedTheRealm.Core.Library;
using FTRShared.Runtime.Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace FeedTheRealm.UI.EditorBar.ElementOption.ItemsMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class WeaponItemCreatorMenuController
    {
        private DropdownField weaponTypeInput;
        private IntegerField damageInput;
        private FloatField attackSpeedInput;
        private FloatField rangeInput;
        private IntegerField ammoInput;

        private ItemDataBuilder itemDataBuilder = new ItemDataBuilder();

        // protected override CreatableObjectCategories Category => CreatableObjectCategories.WeaponItem;
        // protected override string ObjectTypeName => "Weapon Item";

        // protected override void InitializeItemSpecificFields(VisualElement root)
        // {
        //     weaponTypeInput = root.Q<DropdownField>("WeaponTypeField");
        //     LogIfNull(weaponTypeInput, "Weapon type dropdown field");

        //     damageInput = root.Q<IntegerField>("DamageField");
        //     LogIfNull(damageInput, "Damage input field");

        //     attackSpeedInput = root.Q<FloatField>("AttackSpeedField");
        //     LogIfNull(attackSpeedInput, "Attack speed input field");

        //     rangeInput = root.Q<FloatField>("RangeField");
        //     LogIfNull(rangeInput, "Range input field");

        //     ammoInput = root.Q<IntegerField>("AmmoField");
        //     LogIfNull(ammoInput, "Ammo input field");

        //     if (weaponTypeInput != null)
        //     {
        //         weaponTypeInput.choices = Enum.GetNames(typeof(WeaponType)).ToList();
        //     }
        // }

        // protected override void PopulateFields()
        // {
        //     nameInput.value = currentObject.name;
        //     descriptionInput.value = currentObject.description ?? "";
        //     weaponTypeInput.value = currentObject.weaponType.ToString();
        //     damageInput.value = currentObject.damage;
        //     attackSpeedInput.value = currentObject.attackSpeed;
        //     rangeInput.value = currentObject.range;
        //     ammoInput.value = currentObject.ammo;
        //     LoadExistingSprite(currentObject.spriteFile);
        // }

        // protected override bool ValidateSpecificFields()
        // {
        //     if (string.IsNullOrEmpty(weaponTypeInput?.value))
        //     {
        //         ShowValidationError("Weapon type is required");
        //         return false;
        //     }
        //     return true;
        // }

        // protected override void CreateNewObject()
        // {
        //     string savedSpritePath = SaveSpriteIfNeeded();

        //     var weaponItemData = itemDataBuilder
        //         .SetItemData(
        //             null,
        //             nameInput.value,
        //             descriptionInput.value ?? "",
        //             savedSpritePath ?? ""
        //         )
        //         .BuildWeaponItem(
        //             (WeaponType)Enum.Parse(typeof(WeaponType), weaponTypeInput.value),
        //             damageInput.value,
        //             attackSpeedInput.value,
        //             rangeInput.value,
        //             ammoInput.value
        //         );

        //     currentObject = new WeaponItem(weaponItemData);
        //     creatorObjectLibrary.AddCreatable(Category, currentObject);
        //     logger?.Log(
        //         $"Created new weapon item: {currentObject.DisplayName}",
        //         this,
        //         Logging.LogType.Info
        //     );
        // }

        // protected override void UpdateExistingObject()
        // {
        //     string savedSpritePath = SaveSpriteIfNeeded();

        //     currentObject.name = nameInput.value;
        //     currentObject.description = descriptionInput.value;
        //     currentObject.weaponType = (WeaponType)
        //         Enum.Parse(typeof(WeaponType), weaponTypeInput.value);
        //     currentObject.damage = damageInput.value;
        //     currentObject.attackSpeed = attackSpeedInput.value;
        //     currentObject.range = rangeInput.value;
        //     currentObject.ammo = ammoInput.value;
        //     currentObject.spriteFile = savedSpritePath ?? currentObject.spriteFile;
        //     logger?.Log(
        //         $"Updated weapon item: {currentObject.DisplayName}",
        //         this,
        //         Logging.LogType.Info
        //     );
        // }
    }
}
