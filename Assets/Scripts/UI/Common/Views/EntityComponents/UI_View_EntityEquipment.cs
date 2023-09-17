using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public class UI_View_EntityEquipment : UI_View_BaseEntityComponent
    {
        [SerializeField] private bool Interactable;

        [SerializeField] private UI_EquipmentCategory Weapon;
        [SerializeField] private UI_EquipmentCategory Armor;
        [SerializeField] private UI_EquipmentCategory Accessories;

        public override void Initialize(Entity entity)
        {
        }

        public override void Initialize(PlayableCharacter character, PresetSlot preset)
        {
            base.Initialize(character, preset);

            Refresh(character.EquipmentSlots, preset);
        }

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

        public override void Refresh()
        {
            throw new System.NotImplementedException();
        }

        public override void Refresh(PresetSlot preset)
        {
            throw new System.NotImplementedException();
        }

        public override void Preview(PresetSlot preset, Slot slot, Equipment equipment)
        {
            throw new System.NotImplementedException();
        }

        public override void Unpreview()
        {
            throw new System.NotImplementedException();
        }
    }
}
