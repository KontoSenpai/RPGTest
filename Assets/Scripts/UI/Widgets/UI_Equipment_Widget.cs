using RPGTest.Enums;
using RPGTest.Models;
using UnityEngine;

namespace RPGTest.UI.Widgets
{
    public class UI_Equipment_Widget : MonoBehaviour
    {
        [SerializeField] private UI_EquipmentCategory Weapon;

        [SerializeField] private UI_EquipmentCategory Armor;

        [SerializeField] private UI_EquipmentCategory Accessories;

        public void Clear()
        {
            Weapon.Clear();
            Armor.Clear();
            Accessories.Clear();
        }

        public void Refresh(EquipmentSlots equipmentSlots, PresetSlot slot)
        {
            var presetSlots = equipmentSlots.GetEquipmentPreset(slot);

            Weapon.Refresh(presetSlots[Slot.LeftHand], presetSlots[Slot.RightHand]);
            Armor.Refresh(presetSlots[Slot.Head], presetSlots[Slot.Body]);
            Accessories.Refresh(presetSlots[Slot.Accessory1], presetSlots[Slot.Accessory2]);
        }
    }
}
