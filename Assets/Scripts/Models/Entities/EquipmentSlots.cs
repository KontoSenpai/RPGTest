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
        Second,
        None
    }

    [Serializable]
    public class EquipmentPreset
    {
        public Dictionary<Slot, Equipment> Equipment = new Dictionary<Slot, Equipment>()
        {
            { Slot.LeftHand, null },
            { Slot.RightHand, null },
            { Slot.Head, null },
            { Slot.Body, null },
            { Slot.Accessory1, null },
            { Slot.Accessory2, null },
        };

        public Dictionary<Slot, Equipment> GetWeapons()
        {
            return new Dictionary<Slot, Equipment>()
            {
                {Slot.LeftHand, Equipment[Slot.LeftHand] },
                {Slot.RightHand, Equipment[Slot.RightHand] }
            };
        }

        public bool IsEquipped(string equipmentId)
        {
            return Equipment.Values.WhereNotNull().Any(x => x.Id == equipmentId);
        }

        public IEnumerable<Slot> GetEquippedSlots(string equipmentId)
        {
            return Equipment.WhereNotNull(x => x.Value).Where(x => x.Value.Id == equipmentId).Select(x => x.Key);
        }

        public bool TryEquip(Slot slot, Equipment equipment)
        {
            Equipment[slot] = equipment;
            return Equipment[slot] == equipment;
        }

        public bool TryUnequip(Slot primarySlot, Slot secondarySlot, out List<Item> outRemovedEquipments)
        {
            outRemovedEquipments = new List<Item>();
            bool result = false;
            if (TryUnequipInternal(primarySlot, out Equipment removedEquipment))
            {
                if (removedEquipment != null)
                {
                    outRemovedEquipments.Add(removedEquipment);
                }
                result = Equipment[primarySlot] == null;
            }
            if (secondarySlot != Slot.None && TryUnequipInternal(secondarySlot, out removedEquipment))
            {
                if (removedEquipment != null)
                {
                    outRemovedEquipments.Add(removedEquipment);
                }
                result = Equipment[secondarySlot] == null;
            }
            return result;
        }

        public bool TryUnequip(Slot slot, out List<Item> outRemovedEquipments)
        {
            return TryUnequip(slot, Slot.None, out outRemovedEquipments);
        }
        
        private bool TryUnequipInternal(Slot slot, out Equipment removedEquipment)
        {
            removedEquipment = Equipment[slot];
            Equipment[slot] = null;
            return Equipment[slot] == null;
        }
    }

    [Serializable]
    public class EquipmentSlots
    {
        public Dictionary<PresetSlot, EquipmentPreset> Presets { get; private set; }
        public PresetSlot CurrentPreset { get; private set; }

        public EquipmentSlots()
        {
            Presets = new Dictionary<PresetSlot, EquipmentPreset>()
            {
                { PresetSlot.First, new EquipmentPreset() },
                { PresetSlot.Second, new EquipmentPreset() },
            };
        }

        public void ChangePreset()
        {
            CurrentPreset = CurrentPreset == PresetSlot.First ? PresetSlot.Second: PresetSlot.First;
        }

        public Dictionary<Slot, Equipment> GetCurrentWeaponPreset()
        {
            return GetWeaponPreset(CurrentPreset);
        }

        public Dictionary<Slot, Equipment> GetWeaponPreset(PresetSlot preset)
        {
            if (!Presets.TryGetValue(preset, out var equipmentPreset))
            {
                throw new Exception("Unable to retrieve weapon presets");
            }
            return equipmentPreset.GetWeapons();
        }

        public Dictionary<Slot, Equipment> GetCurrentEquipmentPreset()
        {
            return GetEquipmentPreset(CurrentPreset);

        }

        public Dictionary<Slot, Equipment> GetEquipmentPreset(PresetSlot slot)
        {
            if (!Presets.TryGetValue(CurrentPreset, out var equipmentPreset))
            {
                throw new Exception("Unable to retrieve weapon presets");
            }
            return equipmentPreset.Equipment;
        }


        public bool TryEquip(PresetSlot presetSlot, Slot slot, Equipment newEquipment, out List<Item> removedEquipments)
        {
            removedEquipments = new List<Item>();

            if (!Presets.TryGetValue(presetSlot, out var equipmentPreset))
            {
                throw new Exception("Unable to retrieve weapon presets");
            }

            switch (slot)
            {
                case Slot.LeftHand:
                    if (!newEquipment.IsWeapon)
                        return false;
                    else if (!equipmentPreset.TryUnequip(slot, newEquipment.TwoHanded ? Slot.RightHand : Slot.None, out removedEquipments))
                        return false;
                    break;
                case Slot.RightHand:
                    if (!newEquipment.IsWeapon)
                        return false;
                    else if (!equipmentPreset.TryUnequip(slot, newEquipment.TwoHanded ? Slot.LeftHand : Slot.None, out removedEquipments))
                        return false;
                    break;
                case Slot.Head:
                    if (!newEquipment.IsHeadArmor)
                        return false;
                    else if (!equipmentPreset.TryUnequip(slot, out removedEquipments))
                        return false;
                    break;
                case Slot.Body:
                    if (!newEquipment.IsBodyArmor)
                        return false;
                    else if (!equipmentPreset.TryUnequip(slot, out removedEquipments))
                        return false;
                    break;
                case Slot.Accessory1:
                case Slot.Accessory2:
                    if (!newEquipment.IsAccessory)
                        return false;
                    else if (!equipmentPreset.TryUnequip(slot, out removedEquipments))
                        return false;
                    break;
            }

            return equipmentPreset.TryEquip(slot, newEquipment);
        }

        public bool TryUnequip(PresetSlot presetSlot, Slot slot, out List<Item> removedEquipments)
        {
            removedEquipments = new List<Item>();

            if (!Presets.TryGetValue(presetSlot, out var equipmentPreset))
            {
                throw new Exception("Unable to retrieve weapon presets");
            }

            return equipmentPreset.TryUnequip(slot, out removedEquipments);
        }

        public bool IsEquiped(string id)
        {
            foreach(var preset in Presets)
            {
                if(preset.Value.IsEquipped(id))
                {
                    return true;
                }
            }
            return false;
        }

        public Dictionary<PresetSlot, IEnumerable<Slot>> GetEquipedSlots(string equipmentId)
        {
            var equippedSlots = new Dictionary<PresetSlot, IEnumerable<Slot>>();
            foreach(var equipmentPreset in Presets)
            {
                equippedSlots.Add(equipmentPreset.Key, equipmentPreset.Value.GetEquippedSlots(equipmentId));
            }
            return equippedSlots;
        }

        public IEnumerable<Slot> GetEquipedSlot(PresetSlot presetSlot, string equipmentId)
        {
            if (!Presets.TryGetValue(presetSlot, out var equipmentPreset))
            {
                throw new Exception("Unable to retrieve weapon presets");
            }
            return equipmentPreset.GetEquippedSlots(equipmentId);
        }
    }
}
