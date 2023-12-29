using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGTest.Models.Entity.Components
{
    /// <summary>
    /// EquipmentComponent is a component to use for internal Operations on an <see cref="Entity"/> <see cref="Equipment"/>
    /// </summary>
    public class EquipmentComponent
    {
        public PresetSlot CurrentPreset { get; private set; }

        public List<EquipmentPreset> Presets { get; private set; } = new List<EquipmentPreset>()
        {
            new EquipmentPreset(),
            new EquipmentPreset(),
        };

        public EquipmentComponent()
        {

        }

        #region Getters
        /// <summary>
        /// Get the Equipments equipped in given preset.
        /// If no preset is given, use the current preset
        /// </summary>
        /// <param name="slot">Preset to use</param>
        /// <returns>Equipment equipped in given preset</returns>
        public Dictionary<EquipmentSlot, Equipment> GetEquipmentSlots(PresetSlot slot = PresetSlot.None)
        {
            return GetEquipmentSlotsInternal(slot);
        }

        /// <summary>
        /// Get the Weapons equipped in given preset.
        /// If no preset is given, use the current preset
        /// </summary>
        /// <param name="slot">Preset to use</param>
        /// <returns>Weapons equipped in given preset</returns>
        public Dictionary<EquipmentSlot, Equipment> GetWeaponSlots(PresetSlot slot = PresetSlot.None)
        {
            return GetWeaponSlotsInternal(slot);
        }

        /// <summary>
        /// Calculates and returns a random value between the lower and upper range of damage variance provided by the equipment
        /// </summary>
        /// <returns></returns>
        public float GetEquipmentPowerVariance()
        {
            var preset = GetWeaponSlotsInternal(CurrentPreset);

            preset.TryGetValue(EquipmentSlot.LeftHand, out Equipment leftWeapon);
            preset.TryGetValue(EquipmentSlot.RightHand, out Equipment rightWeapon);

            if (leftWeapon != null && rightWeapon != null)
            {
                float leftValue = leftWeapon.PowerRange.GetValue();
                float rightValue = rightWeapon.PowerRange.GetValue();
                return (leftValue + rightValue) / 2;
            }
            else if (leftWeapon != null && rightWeapon == null)
            {
                return leftWeapon.PowerRange.GetValue();
            }
            else if (rightWeapon != null && leftWeapon == null)
            {
                return rightWeapon.PowerRange.GetValue();
            }
            return -1.0f;
        }

        /// <summary>
        /// Returns for each preset, the list of slots where given equipment is equipped
        /// </summary>
        /// <param name="equipmentId"></param>
        /// <returns></returns>
        public Dictionary<PresetSlot, IEnumerable<EquipmentSlot>> GetEquipedSlots(string equipmentId)
        {
            var equippedSlots = new Dictionary<PresetSlot, IEnumerable<EquipmentSlot>>();
            foreach (var equipmentPreset in Presets)
            {
                equippedSlots.Add(equipmentPreset.Preset, equipmentPreset.GetEquippedSlots(equipmentId));
            }
            return equippedSlots;
        }

        /// <summary>
        /// Returns weither or not given equipment is equipped in either presets
        /// </summary>
        /// <param name="equipmentId"></param>
        /// <returns></returns>
        public bool IsEquiped(string equipmentId)
        {
            return Presets.All(p => p.IsEquipped(equipmentId));
        }
        #endregion


        /// <summary>
        /// Change which preset is to be used for calculations
        /// </summary>
        public void ChangePreset()
        {
            CurrentPreset = CurrentPreset == PresetSlot.First ? PresetSlot.Second : PresetSlot.First;
        }

        /// <summary>
        /// Tries to equip a piece of equipment in given preset and slot
        /// </summary>
        /// <param name="preset">Preset to equip the equipment on</param>
        /// <param name="slot">Slot to equip the equipment on</param>
        /// <param name="equipPiece">Equipment to use</param>
        /// <param name="removedEquipments">List of equipments that were removed/replace</param>
        /// <returns>True if the operation is sucessful, false otherwise</returns>
        public bool TryEquip(PresetSlot preset, EquipmentSlot slot, Equipment equipPiece, out List<Item> removedEquipments)
        {
            return TryEquipInternal(preset, slot, equipPiece, out removedEquipments);
        }

        /// <summary>
        /// Tries to unequip a list of equipment in given preset and slot
        /// </summary>
        /// <param name="preset">Preset to equip the equipment on</param>
        /// <param name="slot">Slot to equip the equipment on</param>
        /// <param name="equipPiece">Equipment to use</param>
        /// <param name="removedEquipments">List of equipments that were removed/replace</param>
        /// <returns>True if the operation is sucessful, false otherwise</returns>
        public bool TryUnequip(PresetSlot preset, EquipmentSlot slot, out List<Item> removedEquipments)
        {
            TryUnequipInternal(preset, slot, out removedEquipments);
            removedEquipments.WhereNotNull();
            return removedEquipments.Any();
        }

        #region private methods
        private EquipmentPreset GetPresetInternal(PresetSlot preset)
        {
            preset = preset == PresetSlot.None ? CurrentPreset : preset;
            return Presets.Single(p => p.Preset == preset);
        }

        private Dictionary<EquipmentSlot, Equipment> GetWeaponSlotsInternal(PresetSlot preset)
        {
            var weaponIds = GetPresetInternal(preset).GetWeapons();
            return ConvertEquipmentSlotIdsToEquipmentSlotModels(weaponIds);
        }

        private Dictionary<EquipmentSlot, Equipment> GetEquipmentSlotsInternal(PresetSlot preset)
        {
            var equipmentIds = GetPresetInternal(preset).GetEquipment();
            return ConvertEquipmentSlotIdsToEquipmentSlotModels(equipmentIds);
        }

        private bool TryEquipInternal(PresetSlot presetSlot, EquipmentSlot slot, Equipment newEquipment, out List<Item> removedEquipments)
        {
            removedEquipments = new List<Item>();
            var removedEquipmentIds = new List<string>();
            var equipmentPreset = Presets.Single(p => p.Preset == presetSlot);

            switch (slot)
            {
                case EquipmentSlot.LeftHand:
                    if (!newEquipment.IsWeapon)
                        return false;
                    else if (!equipmentPreset.TryUnequip(slot, newEquipment.TwoHanded ? EquipmentSlot.RightHand : EquipmentSlot.None, out removedEquipmentIds))
                        return false;
                    break;
                case EquipmentSlot.RightHand:
                    if (!newEquipment.IsWeapon)
                        return false;
                    else if (!equipmentPreset.TryUnequip(slot, newEquipment.TwoHanded ? EquipmentSlot.LeftHand : EquipmentSlot.None, out removedEquipmentIds))
                        return false;
                    break;
                case EquipmentSlot.Head:
                    if (!newEquipment.IsHeadArmor)
                        return false;
                    else if (!equipmentPreset.TryUnequip(slot, EquipmentSlot.None, out removedEquipmentIds))
                        return false;
                    break;
                case EquipmentSlot.Body:
                    if (!newEquipment.IsBodyArmor)
                        return false;
                    else if (!equipmentPreset.TryUnequip(slot, EquipmentSlot.None, out removedEquipmentIds))
                        return false;
                    break;
                case EquipmentSlot.Accessory1:
                case EquipmentSlot.Accessory2:
                    if (!newEquipment.IsAccessory)
                        return false;
                    else if (!equipmentPreset.TryUnequip(slot, EquipmentSlot.None, out removedEquipmentIds))
                        return false;
                    break;
            }

            removedEquipments = removedEquipmentIds.Select(i => ItemCollector.TryGetItem(i)).ToList();

            return equipmentPreset.TryEquip(slot, newEquipment.Id);
        }

        private bool TryUnequipInternal(PresetSlot presetSlot, EquipmentSlot slot, out List<Item> removedEquipments)
        {
            var equipmentPreset = Presets.Single(p => p.Preset == presetSlot);
            var unequippedSuccesful = equipmentPreset.TryUnequip(slot, EquipmentSlot.None, out var removedEquipmentIds);

            removedEquipments = removedEquipmentIds.Select(i => ItemCollector.TryGetItem(i)).ToList();
            return unequippedSuccesful;
        }

        private Dictionary<EquipmentSlot, Equipment> ConvertEquipmentSlotIdsToEquipmentSlotModels(Dictionary<EquipmentSlot, string> equipmentSlotIds)
        {
            var equipmentSlotModels = new Dictionary<EquipmentSlot, Equipment>();
            foreach (var equipmentSlotId in equipmentSlotIds)
            {
                if (string.IsNullOrEmpty(equipmentSlotId.Value))
                {
                    equipmentSlotModels.Add(equipmentSlotId.Key, null);
                }
                else if (ItemCollector.TryGetEquipment(equipmentSlotId.Value, out Equipment equipment))
                {
                    equipmentSlotModels.Add(equipmentSlotId.Key, equipment);
                }
            }
            return equipmentSlotModels;
        }
        #endregion
    }
}