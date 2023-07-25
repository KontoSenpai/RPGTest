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
                guiItem.gameObject.name += "_E";

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

            var unequippedQuantity = GetTotalUnequippedQuantity(quantity, equippedItems);
            var unequippedGuiItem = equipmentGuiItems.SingleOrDefault((g) => g.GetOwner() == null);

            if (unequippedGuiItem != null) // Update existing unequipped guiItem
            {
                unequippedGuiItem.UpdateHeldQuantity(unequippedQuantity);
                if (unequippedGuiItem.Quantity == -1)
                {
                    guiCDs.Add(unequippedGuiItem.gameObject);
                }
            }
            else if (unequippedGuiItem == null && unequippedQuantity > 0) // Create guiItem
            {
                guiItems.Add(InstantiateUnequippedEquipment(item, unequippedQuantity));
            }

            // Check for equipped equipments that require a new gui item
            foreach(var equippedItem in equippedItems)
            {
                if (TryCreateGuiItemsForCharacter(item, equippedItem, equipmentGuiItems, out var createdGuiItems))
                {
                    guiCDs.AddRange(createdGuiItems);
                }
            }

            if (TryDeleteGuiItems(equippedItems, equipmentGuiItems, out var deletedGuiItems))
            {
                guiCDs.AddRange(deletedGuiItems);
            }

            return guiCDs;
        }

        private List<GameObject> InstantiateEquipmentForOwner(Item item, KeyValuePair<PlayableCharacter, Dictionary<PresetSlot, IEnumerable<Slot>>> owner)
        {
            var guiItems = new List<GameObject>();
            foreach (var equipmentSet in owner.Value)
            {
                foreach (var slot in equipmentSet.Value)
                {
                    var guiItem = InstantiateItemInternal(item);
                    guiItem.gameObject.name += $"_O{owner.Key.Id}_P{equipmentSet.Key}_S{slot}";
                    guiItem.GetComponent<UI_InventoryItem>().InitializeForOwner(item, owner.Key, equipmentSet.Key, slot);
                    guiItems.Add(guiItem);
                }
            }
            return guiItems;
        }

        private GameObject InstantiateUnequippedEquipment(Item item, int quantity)
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
        /// Create new guiItems for given owner if none exists with instantiator rules
        /// </summary>
        /// <param name="item">Item to evaluate</param>
        /// <param name="owner">KeyValuePair with an owner as key, and its equipment sets containing item</param>
        /// <param name="equipmentGuiItems">Existing GuiItems with given item</param>
        /// <param name="createdGuiItems">GuiItems created within the method</param>
        /// <returns></returns>
        private bool TryCreateGuiItemsForCharacter(Item item, KeyValuePair<PlayableCharacter, Dictionary<PresetSlot, IEnumerable<Slot>>> owner, IEnumerable<UI_InventoryItem> equipmentGuiItems, out List<GameObject> createdGuiItems)
        {
            createdGuiItems = new List<GameObject>();
            if (StackOwnersOnSingleGuiItem)
            {
                var ownedEquipmentGuiItem = equipmentGuiItems.SingleOrDefault((i) => i.Owners.Any());
                if (ownedEquipmentGuiItem == null) // if there's no concatenated gui item, create one 
                {
                    var guiItem = InstantiateItemInternal(item);
                    guiItem.GetComponent<UI_InventoryItem>().InitializeForOwners(item, new List<PlayableCharacter>() { owner.Key });
                    createdGuiItems.Add(guiItem);
                    return true;
                } 
                else if (!ownedEquipmentGuiItem.Owners.Contains(owner.Key)) // if there's a concatenated gui item but it doesn't contains owner, append it but don't create
                {
                    ownedEquipmentGuiItem.Owners.Add(owner.Key);
                }
                return false;
            }

            var guiItemsOwnedByCharacter = equipmentGuiItems.Where((i) => i.Owners.Contains(owner.Key));
            if (!guiItemsOwnedByCharacter.Any()) // if there's no gui items for owner, create them
            {
                createdGuiItems.AddRange(InstantiateEquipmentForOwner(item, owner));
                return true;
            }

            foreach(var equipmentSet in owner.Value)
            {
                foreach (var slot in equipmentSet.Value)
                {
                    if (guiItemsOwnedByCharacter.Any((i) => i.Preset == equipmentSet.Key && i.Slot == slot))
                    {
                        continue;
                    }

                    var guiItem = InstantiateItemInternal(item);
                    guiItem.GetComponent<UI_InventoryItem>().InitializeForOwner(item, owner.Key, equipmentSet.Key, slot);
                    createdGuiItems.Add(guiItem);
                }
            }

            return createdGuiItems.Any();
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
        private bool TryDeleteGuiItems(Dictionary<PlayableCharacter, Dictionary<PresetSlot, IEnumerable<Slot>>> owners, IEnumerable<UI_InventoryItem> equipmentGuiItems, out List<GameObject> deletedGuiItems)
        {
            deletedGuiItems = new List<GameObject>();
            if (StackOwnersOnSingleGuiItem)
            {
                var ownedEquipmentGuiItem = equipmentGuiItems.SingleOrDefault((i) => i.Owners.Any());
                if (ownedEquipmentGuiItem != null && !owners.Any())
                {
                    deletedGuiItems.Add(ownedEquipmentGuiItem.gameObject);
                    return true;
                }
                else if (ownedEquipmentGuiItem != null && owners.Any())
                {
                    ownedEquipmentGuiItem.UpdateHeldQuantity(owners.Keys.Count);
                    ownedEquipmentGuiItem.SetOwners(owners.Keys.ToList());
                }
                return false;
            }

            foreach(var equipmentGuiItem in equipmentGuiItems.Where((i) => i.GetOwner() != null))
            {
                if (!owners.ContainsKey(equipmentGuiItem.GetOwner()))
                {
                    deletedGuiItems.Add(equipmentGuiItem.gameObject);
                    continue;
                }
                if (!owners.TryGetValue(equipmentGuiItem.GetOwner(), out var equippedSlots))
                {
                    throw new Exception("Unexpected Error");
                }

                foreach (var presetSlots in equippedSlots)
                {
                    if (equipmentGuiItem.Preset == presetSlots.Key && presetSlots.Value.Contains(equipmentGuiItem.Slot))
                    {
                        return false;
                    }
                }
            }

            deletedGuiItems.ForEach((i) => i.GetComponent<UI_InventoryItem>().UpdateHeldQuantity(0));
            return deletedGuiItems.Any();
        }
    }
}
