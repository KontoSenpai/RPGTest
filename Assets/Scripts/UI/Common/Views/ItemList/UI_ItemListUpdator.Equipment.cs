using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public partial class UI_ItemListUpdator : MonoBehaviour
    {
        private List<GameObject> InstantiateEquipment(Item item, int quantity)
        {
            var guiItems = new List<GameObject>();

            // Retrieve potential owners of equipment
            var equippedItems = GetEquipmentOwners(item.Id);
            if (StackOwnersOnSingleGuiItem)
            {
                var owners = equippedItems.GroupBy((e) => e.Key).Select((e) => e.First().Key).ToList();
                var guiItem = InstantiateItemInternal(item);

                guiItem.GetComponent<UI_InventoryItem>().InitializeForOwners(item, owners);
                guiItems.Add(guiItem);
            } 
            else
            {
                foreach(var equippedItem in equippedItems)
                {
                    guiItems.AddRange(InstantiateEquipmentForOwner(item, equippedItem));
                }
            }

            var unequippedQuantity = GetTotalUnequippedQuantity(quantity, equippedItems);
            if (unequippedQuantity > 0)
            {
                guiItems.Add(InstantiateUnequippedEquipment(item, unequippedQuantity));
            }

            return guiItems;
        }
        
        private List<GameObject> UpdateEquipment(List<GameObject> guiItems, Item item, int quantity)
        {
            var guiCDs = new List<GameObject>();
            // Retrieve potential owners of equipment
            Dictionary<PlayableCharacter, Dictionary<PresetSlot, IEnumerable<Slot>>> equippedItems = GetEquipmentOwners(item.Id);

            var equipmentGuiItems = guiItems
                .Where((i) => i.GetComponent<UI_InventoryItem>().Item.Id == item.Id)
                .Select((i) => i.GetComponent<UI_InventoryItem>());

            if (StackOwnersOnSingleGuiItem)
            {
                var ownersGui = equipmentGuiItems.SingleOrDefault((i) => i.Owners.Count > 0);
                if (ownersGui != null)
                {
                    ownersGui.SetOwners(equippedItems.GroupBy((e) => e.Key).Select((e) => e.First().Key).ToList());
                } else if(ownersGui == null && equippedItems.Count > 0)
                {
                    var guiItem = InstantiateItemInternal(item);
                    guiItem.GetComponent<UI_InventoryItem>().InitializeForOwners(item, equippedItems.GroupBy((e) => e.Key).Select((e) => e.First().Key).ToList());
                    guiCDs.Add(guiItem);
                }
            }
            else
            {
                // First, update or create gui items for unequipped equipment
                var unequippedQuantity = GetTotalUnequippedQuantity(quantity, equippedItems);
                var unequippedGuiItem = equipmentGuiItems.SingleOrDefault((g) => g.GetOwner() == null);

                if (unequippedGuiItem != null)
                {
                    unequippedGuiItem.UpdateHeldQuantity(unequippedQuantity);
                    if (unequippedGuiItem.Quantity == -1)
                    {
                        guiCDs.Add(unequippedGuiItem.gameObject);
                    }

                }
                else if (unequippedGuiItem == null && unequippedQuantity > 0)
                {
                    guiItems.Add(InstantiateUnequippedEquipment(item, unequippedQuantity));
                }

                // Loop over existing GuiItems to find ones that need deletion
                foreach (var guiItem in equipmentGuiItems.Where(g => g.GetOwner() != null))
                {
                    if (ShouldDeleteGuiItemForEquipment(guiItem, equippedItems))
                    {
                        guiItem.UpdateHeldQuantity(0);
                        guiCDs.Add(guiItem.gameObject);
                    }
                }
            }
            return guiCDs;
        }

        protected List<GameObject> InstantiateEquipmentForOwner(Item item, KeyValuePair<PlayableCharacter, Dictionary<PresetSlot, IEnumerable<Slot>>> equippedItem)
        {
            var guiItems = new List<GameObject>();
            foreach (var presetSlots in equippedItem.Value)
            {
                foreach (var slot in presetSlots.Value)
                {
                    var guiItem = InstantiateItemInternal(item);
                    guiItem.GetComponent<UI_InventoryItem>().InitializeForOwner(item, equippedItem.Key, presetSlots.Key, slot);
                    guiItems.Add(guiItem);
                }
            }
            return guiItems;
        }

        protected GameObject InstantiateUnequippedEquipment(Item item, int quantity)
        {
            var unequippedGuiItem = InstantiateItemInternal(item);
            unequippedGuiItem.GetComponent<UI_InventoryItem>().Initialize(item, quantity);

            return unequippedGuiItem;
        }

        /// <summary>
        /// Retrieve which playable character is currently equipping given equipment piece
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private Dictionary<PlayableCharacter, Dictionary<PresetSlot, IEnumerable<Slot>>> GetEquipmentOwners(string id)
        {
            return m_partyManager.GetAllExistingPartyMembers().Where(p => p.EquipmentSlots.IsEquiped(id)).ToDictionary(p => p, p => p.EquipmentSlots.GetEquipedSlots(id));
        }

        /// <summary>
        /// Get the amount of the current given object that is currently unequipped
        /// </summary>
        /// <param name="quantity"></param>
        /// <param name="equippedItems"></param>
        /// <returns></returns>
        private int GetTotalUnequippedQuantity(int quantity, Dictionary<PlayableCharacter, Dictionary<PresetSlot, IEnumerable<Slot>>> equippedItems)
        {
            var unequippedQuantity = quantity;
            foreach (var equippedItem in equippedItems)
            {
                foreach (var presetSlot in equippedItem.Value)
                {
                    unequippedQuantity -= presetSlot.Value.Count();
                }
            }
            return unequippedQuantity;
        }

        /// <summary>
        /// Evaluate if a GuiItem should be deleted.
        /// A GuiItem should be deleted under these conditions:
        /// 1. Its Owner is no longer preset in the EquippedItems dictionary
        /// 2. A owner is here, but none of its preset contains it.
        /// </summary>
        /// <param name="guiItem"></param>
        /// <param name="equippedItems"></param>
        /// <returns></returns>
        private bool ShouldDeleteGuiItemForEquipment(UI_InventoryItem guiItem, Dictionary<PlayableCharacter, Dictionary<PresetSlot, IEnumerable<Slot>>> equippedItems)
        {
            if (!equippedItems.ContainsKey(guiItem.GetOwner()))
            {
                return true;
            }

            if (!equippedItems.TryGetValue(guiItem.GetOwner(), out var equippedSlots))
            {
                throw new Exception("Unexpected Error");
            }
            foreach (var presetSlots in equippedSlots)
            {
                if (guiItem.Preset == presetSlots.Key && presetSlots.Value.Contains(guiItem.Slot))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
