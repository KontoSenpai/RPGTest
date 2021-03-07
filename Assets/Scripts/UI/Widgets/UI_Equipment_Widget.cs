using RPGTest.Enums;
using RPGTest.Models;
using UnityEngine;

namespace RPGTest.UI.Widgets
{
    public class UI_Equipment_Widget : MonoBehaviour
    {
        [SerializeField] private UI_EquipmentCategory FirstPreset;
        [SerializeField] private UI_EquipmentCategory SecondPreset;

        [SerializeField] private UI_EquipmentCategory Armor;

        [SerializeField] private UI_EquipmentCategory Accessories;

        public void Refresh(EquipmentSlots equipmentSlots)
        {
            if(equipmentSlots == null)
            {
                FirstPreset.Clear();
                SecondPreset.Clear();

                Armor.Clear();

                Accessories.Clear();
            }
            else
            {
                FirstPreset.Refresh(equipmentSlots.Equipment[Slot.LeftHand1], equipmentSlots.Equipment[Slot.RightHand1]);
                SecondPreset.Refresh(equipmentSlots.Equipment[Slot.LeftHand2], equipmentSlots.Equipment[Slot.RightHand2]);

                Armor.Refresh(equipmentSlots.Equipment[Slot.Head], equipmentSlots.Equipment[Slot.Body]);

                Accessories.Refresh(equipmentSlots.Equipment[Slot.Accessory1], equipmentSlots.Equipment[Slot.Accessory2]);
            }
        }
    }
}
