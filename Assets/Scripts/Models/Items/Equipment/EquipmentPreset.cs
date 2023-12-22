using RPGTest.Enums;
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
        public PresetSlot Preset { get; set; }

        public string LeftHand { get; set; } = String.Empty;

        public string RightHand { get; set; } = String.Empty;

        public string Head { get; set; } = String.Empty;

        public string Body { get; set; } = String.Empty;

        public string Accessory1 { get; set; } = String.Empty;

        public string Accessory2 { get; set; } = String.Empty;

        public EquipmentPreset() { }
        public EquipmentPreset(PresetSlot preset)
        {
            Preset = preset;
        }

        #region Getters
        /// <summary>
        /// Gets the weapon slots and their equipment
        /// </summary>
        /// <returns>A dictionary of slots and the weapons in those slots</returns>
        public Dictionary<EquipmentSlot, string> GetWeapons()
        {
            return new Dictionary<EquipmentSlot, string>()
            {
                {EquipmentSlot.LeftHand, LeftHand },
                {EquipmentSlot.RightHand, RightHand }
            };
        }

        /// <summary>
        /// Gets the equipment slots and their equipment
        /// </summary>
        /// <returns>A dictionary of slot and the equipment in those slots</returns>
        public Dictionary<EquipmentSlot, string> GetEquipment()
        {
            return new Dictionary<EquipmentSlot, string>()
            {
                { EquipmentSlot.LeftHand, LeftHand },
                { EquipmentSlot.RightHand, RightHand },
                { EquipmentSlot.Head, Head },
                { EquipmentSlot.Body, Body },
                { EquipmentSlot.Accessory1, Accessory1 },
                { EquipmentSlot.Accessory2, Accessory2 },
            };
        }

        /// <summary>
        /// Returns the list of slots where the equipment is equipped on
        /// </summary>
        /// <param name="equipmentId"></param>
        /// <returns>List of slots where the equipmentId match</returns>
        public IEnumerable<EquipmentSlot> GetEquippedSlots(string equipmentId)
        {
            return GetEquipment().Where(x => x.Value == equipmentId).Select(x => x.Key);
        }

        /// <summary>
        /// Determines if give piece of equipment is equipped or not
        /// </summary>
        /// <param name="equipmentId"></param>
        /// <returns>true if the equipment is used, false otherwise</returns>
        public bool IsEquipped(string equipmentId)
        {
            return GetEquipment().Any(x => x.Value == equipmentId);
        }
        #endregion

        public bool TryEquip(EquipmentSlot slot, string equipmentId)
        {
            return TryEquipInternal(slot, equipmentId);
        }

        public bool TryUnequip(EquipmentSlot primarySlot, EquipmentSlot secondarySlot, out List<string> outRemovedEquipmentIds)
        {
            outRemovedEquipmentIds = new List<string>();
            bool result = false;
            var unequipped = TryUnequipInternal(primarySlot, out string removedEquipmentId);
            if (unequipped) {
                if (removedEquipmentId != string.Empty)
                {
                    outRemovedEquipmentIds.Add(removedEquipmentId);
                }
                result = unequipped;
            }
            if (secondarySlot != EquipmentSlot.None)
            {
                unequipped = TryUnequipInternal(secondarySlot, out removedEquipmentId);
                if (removedEquipmentId != string.Empty)
                {
                    outRemovedEquipmentIds.Add(removedEquipmentId);
                }
                result = unequipped;
            }
            return result;
        }

        #region private methods
        private bool TryChangeEquipmentInternal(EquipmentSlot slot, string equipmentId, out string removedEquipment)
        {
            switch (slot)
            {
                case EquipmentSlot.LeftHand:
                    removedEquipment = LeftHand;
                    LeftHand = equipmentId;
                    break;
                case EquipmentSlot.RightHand:
                    removedEquipment = RightHand;
                    RightHand = equipmentId;
                    break;
                case EquipmentSlot.Head:
                    removedEquipment = Head;
                    Head = equipmentId;
                    break;
                case EquipmentSlot.Body:
                    removedEquipment = Body;
                    Body = equipmentId;
                    break;
                case EquipmentSlot.Accessory1:
                    removedEquipment = Accessory1;
                    Accessory1 = equipmentId;
                    break;
                case EquipmentSlot.Accessory2:
                    removedEquipment = Accessory2;
                    Accessory2 = equipmentId;
                    break;
                default:
                    throw new Exception("Unsupported EquipmentSlot");
            }

            var equipment = GetEquipment();
            return equipment[slot] == equipmentId;
        }

        private bool TryEquipInternal(EquipmentSlot slot, string equipmentId)
        {
            return TryChangeEquipmentInternal(slot, equipmentId, out string unequipped);
        }

        private bool TryUnequipInternal(EquipmentSlot slot, out string removedEquipment)
        {
            var unequippedSuccesfully = TryChangeEquipmentInternal(slot, string.Empty, out removedEquipment);
            return unequippedSuccesfully;
        }
        #endregion
    }
}
