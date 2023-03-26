using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGTest.Models
{

    public enum PresetSlot
    {
        First,
        Second
    }

    [Serializable]
    public class EquipmentSlots
    {
        public Dictionary<Slot, Equipment> Equipment = new Dictionary<Slot, Equipment>()
        {
            { Slot.LeftHand1, null },
            { Slot.RightHand1, null },
            { Slot.LeftHand2, null },
            { Slot.RightHand2, null },
            { Slot.Head, null },
            { Slot.Body, null },
            { Slot.Accessory1, null },
            { Slot.Accessory2, null },
        };

        public PresetSlot CurrentPreset { get; private set; }

        public void ChangePreset()
        {
            switch(CurrentPreset)
            {
                case PresetSlot.First:
                    CurrentPreset = PresetSlot.Second;
                    break;
                case PresetSlot.Second:
                    CurrentPreset = PresetSlot.First;
                    break;
            }
            var preset = GetCurrentWeaponPreset();
        }

        public Dictionary<Slot, Equipment> GetCurrentWeaponPreset()
        {
            return GetWeaponPreset(CurrentPreset);
        }

        public Dictionary<Slot, Equipment> GetWeaponPreset(PresetSlot slot)
        {
            switch (slot)
            {
                case PresetSlot.First:
                    return new Dictionary<Slot, Equipment>()
                    {
                        {Slot.LeftHand, Equipment[Slot.LeftHand1] },
                        {Slot.RightHand, Equipment[Slot.RightHand1] }
                    };
                case PresetSlot.Second:
                    return new Dictionary<Slot, Equipment>()
                    {
                        {Slot.LeftHand, Equipment[Slot.LeftHand2] },
                        {Slot.RightHand, Equipment[Slot.RightHand2] }
                    };
            }
            throw new Exception("Unable to retrieve weapon presets");
        }

        public Dictionary<Slot, Equipment> GetCurrentEquipmentPreset()
        {
            return GetEquipmentPreset(CurrentPreset);
        }

        public Dictionary<Slot, Equipment> GetEquipmentPreset(PresetSlot slot)
        {
            var equipment = GetWeaponPreset(slot);

            equipment.Add(Slot.Head, Equipment[Slot.Head]);
            equipment.Add(Slot.Body, Equipment[Slot.Body]);
            equipment.Add(Slot.Accessory1, Equipment[Slot.Accessory1]);
            equipment.Add(Slot.Accessory2, Equipment[Slot.Accessory2]);

            return equipment;
        }


        public bool TryEquip(Slot slot, Equipment equipment, out List<Item> removedEquipments)
        {
            removedEquipments = new List<Item>();

            switch (slot)
            {
                case Slot.LeftHand1:
                    if (!equipment.IsWeapon)
                        return false;
                    else if (!TryUnequip(Slot.LeftHand1, out removedEquipments, equipment.TwoHanded ? Slot.RightHand1: Slot.None))
                        return false;
                    break;
                case Slot.RightHand1:
                    if (!equipment.IsWeapon)
                        return false;
                    else if (!TryUnequip(Slot.RightHand1, out removedEquipments, equipment.TwoHanded ? Slot.LeftHand1 : Slot.None))
                        return false;
                    break;
                case Slot.LeftHand2:
                    if (!equipment.IsWeapon)
                        return false;
                    else if (!TryUnequip(Slot.LeftHand2, out removedEquipments, equipment.TwoHanded ? Slot.RightHand2 : Slot.None))
                        return false;
                    break;
                case Slot.RightHand2:
                    if (!equipment.IsWeapon)
                        return false;
                    else if (!TryUnequip(Slot.RightHand2, out removedEquipments, equipment.TwoHanded ? Slot.LeftHand2 : Slot.None))
                        return false;
                    break;
                case Slot.Head:
                    if (!equipment.IsHeadArmor)
                        return false;
                    else if (!TryUnequip(Slot.Head, out removedEquipments))
                        return false;
                    break;
                case Slot.Body:
                    if (!equipment.IsBodyArmor)
                        return false;
                    else if (!TryUnequip(Slot.Body, out removedEquipments))
                        return false;
                    break;
                case Slot.Accessory1:
                case Slot.Accessory2:
                    if (!equipment.IsAccessory)
                        return false;
                    else if (!TryUnequip(Slot.Accessory1, out removedEquipments))
                        return false;
                    break;
            }
            Equipment[slot] = equipment;
            return Equipment[slot] == equipment;
        }

        public bool TryUnequip(Slot primarySlot, out List<Item> removedEquipments, Slot secondarySlot = Slot.None)
        {
            removedEquipments = new List<Item>();
            bool result = false;
            if (TryUnequip(primarySlot, out Equipment removedEquipment))
            {
                if(removedEquipment != null)
                {
                    removedEquipments.Add(removedEquipment);
                }
                result = Equipment[primarySlot] == null;
            }
            if(secondarySlot != Slot.None && TryUnequip(secondarySlot, out removedEquipment))
            {
                if(removedEquipment != null)
                {
                    removedEquipments.Add(removedEquipment);
                }
                result = Equipment[secondarySlot] == null;
            }
            return result;
        }

        public bool TryUnequip(Slot slot, out Equipment removedEquipment)
        {
            removedEquipment = Equipment[slot];
            Equipment[slot] = null;
            return Equipment[slot] == null;
        }

        public bool IsEquiped(string id)
        {
            return Equipment.Values.WhereNotNull().Any(x => x.Id == id);
        }

        public int GetEquipedCount(string id)
        {
            return Equipment.Values.WhereNotNull().Where(x => x.Id == id).Count();
        }

        public IEnumerable<Slot> GetEquipedSlot(string id)
        {
            return Equipment.WhereNotNull(x => x.Value).Where(x => x.Value.Id == id).Select(x => x.Key);
        }
    }
}
