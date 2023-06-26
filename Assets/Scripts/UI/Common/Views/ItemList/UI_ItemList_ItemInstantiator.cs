using RPGTest.Enums;
using RPGTest.Managers;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public class UI_ItemList_ItemInstantiator : MonoBehaviour
    {
        [SerializeField] protected GameObject ItemGo;
        [SerializeField] protected bool StackOwnersOnSingleGuiItem;
        [SerializeField] protected bool DisplayItemValue;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        /// <summary>
        /// Will instantiate the required amount of gui items for given item
        /// </summary>
        /// <param name="itemDisplay"></param>
        /// <returns></returns>
        public List<GameObject> InstantiateItem(UIItemDisplay itemDisplay)
        {
            switch (itemDisplay.Item.Type)
            {
                case ItemType.Equipment:
                    return InstantiateEquipment(itemDisplay);
                default:
                    return InstantiateDefault(itemDisplay);
            }
        }

        #region Equipment
        private List<GameObject> InstantiateEquipment(UIItemDisplay itemDisplay)
        {
            var guiItems = new List<GameObject>();
            // Retrieve potential owners of equipment
            var equippedItems = GetEquipmentOwners(itemDisplay.Item.Id);
            if (StackOwnersOnSingleGuiItem)
            {
                var owners = equippedItems.GroupBy((e) => e.Key).Select((e) => e.First().Key).ToList();
                var guiItem = InstantiateItemInternal(itemDisplay.Item);

                guiItem.GetComponent<UI_InventoryItem>().InitializeForOwners(itemDisplay.Item, owners);
                guiItems.Add(guiItem);
            } 
            else
            {
                foreach(var equippedItem in equippedItems)
                {
                    guiItems.AddRange(InstantiateEquipmentForOwner(itemDisplay.Item, equippedItem));
                }
            }
            var unequippedQuantity = GetTotalUnequippedQuantity(itemDisplay.Quantity, equippedItems);
            var unequippedGuiItem = InstantiateItemInternal(itemDisplay.Item);
            unequippedGuiItem.GetComponent<UI_InventoryItem>().Initialize(itemDisplay.Item, unequippedQuantity);
            guiItems.Add(unequippedGuiItem);
            return guiItems;
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

        /// <summary>
        /// Retrieve which playable character is currently equipping given equipment piece
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected Dictionary<PlayableCharacter, Dictionary<PresetSlot, IEnumerable<Slot>>> GetEquipmentOwners(string id)
        {
            return m_partyManager.GetAllExistingPartyMembers().Where(p => p.EquipmentSlots.IsEquiped(id)).ToDictionary(p => p, p => p.EquipmentSlots.GetEquipedSlots(id));
        }

        /// <summary>
        /// Get the amount of the current given object that is currently unequipped
        /// </summary>
        /// <param name="quantity"></param>
        /// <param name="equippedItems"></param>
        /// <returns></returns>
        protected int GetTotalUnequippedQuantity(int quantity, Dictionary<PlayableCharacter, Dictionary<PresetSlot, IEnumerable<Slot>>> equippedItems)
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

        #endregion

        private List<GameObject> InstantiateDefault(UIItemDisplay itemDisplay)
        {
            var guiItem = InstantiateItemInternal(itemDisplay.Item);
            guiItem.GetComponent<UI_InventoryItem>().Initialize(itemDisplay.Item, itemDisplay.Quantity);

            return new List<GameObject> { guiItem };
        }

        /// <summary>
        /// Instantiate a guiItem for given item
        /// </summary>
        /// <param name="item">Item for which to create a GuiItem</param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        private GameObject InstantiateItemInternal(Item item)
        {
            var guiItem = Instantiate(ItemGo);
            guiItem.name = item.Id;
            guiItem.transform.localScale = new Vector3(1, 1, 1);

            return guiItem;
        }
    }
}
