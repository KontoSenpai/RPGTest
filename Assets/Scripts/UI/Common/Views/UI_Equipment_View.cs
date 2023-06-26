using RPGTest.Enums;
using RPGTest.Models;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public class UI_Equipment_View : MonoBehaviour
    {
        [SerializeField] private bool Interactable;

        [SerializeField] private UI_EquipmentCategory Weapon;
        [SerializeField] private UI_EquipmentCategory Armor;
        [SerializeField] private UI_EquipmentCategory Accessories;

        public void InitializeSelection()
        {
            Weapon.InitializeSelection();
        }

        public void Refresh(EquipmentSlots equipmentSlots, PresetSlot slot)
        {
            var presetSlots = equipmentSlots.GetEquipmentPreset(slot);

            Weapon.Refresh(presetSlots[Slot.LeftHand], presetSlots[Slot.RightHand]);
            Armor.Refresh(presetSlots[Slot.Head], presetSlots[Slot.Body]);
            Accessories.Refresh(presetSlots[Slot.Accessory1], presetSlots[Slot.Accessory2]);

            Weapon.SetInteractable(Interactable);
            Armor.SetInteractable(Interactable);
            Accessories.SetInteractable(Interactable);
        }
    }
}
