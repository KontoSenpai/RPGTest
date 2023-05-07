﻿using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Managers;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using RPGTest.UI.InventoryMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.Core.Assets.Scripts.UI.Widgets
{
    public class UI_ItemList : MonoBehaviour
    {
        [SerializeField] private GameObject ItemGo;
        [SerializeField] private GameObject ItemList;

        private InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;
        PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        private int m_currentItemIndex = 0;

        private List<GameObject> m_items = new List<GameObject>();

        // Lambda to retrieve an Item GameObject from an item declaration
        private Func<GameObject, string, bool> m_findGOFunc = (gameO, id) => gameO.GetComponent<UI_SubMenu_Inventory_Item>().GetItem().Id == id;

        [HideInInspector]
        public ItemNavigatedHandler ItemNavigated { get; set; }
        [HideInInspector]
        public delegate void ItemNavigatedHandler(int index);



        #region input events
        public void Navigate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var movement = obj.ReadValue<Vector2>();
            if (movement.y > 0 && m_currentItemIndex > 0)
            {
                m_currentItemIndex -= 1;
            }
            else if (movement.y < 0 && m_currentItemIndex < m_items.Count - 1)
            {
                m_currentItemIndex += 1;
            }
            ItemNavigated(m_currentItemIndex);
        }
        #endregion

        public void Initialize()
        {
            Refresh();

            m_currentItemIndex = 0;
            if (m_items.Count > 0)
            {
                m_items[0].GetComponent<Button>().Select();
            }
        }

        public void Refresh(bool refreshAll = true, List<Item> items = null, ItemType type = ItemType.None)
        {
            if (refreshAll)
            {
                Clear();
            }

            RefreshItems(items);
        }

        public void Clear()
        {
            m_items.ForEach(x => Destroy(x));
            m_items.Clear();
        }

        public IEnumerable<UI_SubMenu_Inventory_Item> GetItems()
        {
            return m_items.Select(x => x.GetComponent<UI_SubMenu_Inventory_Item>());
        }

        public UI_SubMenu_Inventory_Item GetCurrentSelectedItem()
        {
            return m_items[m_currentItemIndex].GetComponent<UI_SubMenu_Inventory_Item>();
        }


        private void RefreshItems(List<Item> items = null)
        {
            if (items == null)
            {
                items = m_inventoryManager.GetAllItems().Keys.Select(x => ItemCollector.TryGetItem(x)).ToList();
            }

            foreach (var item in items)
            {
                int heldQuantity = m_inventoryManager.GetHeldItemQuantity(item.Id);
                switch (item.Type)
                {
                    case ItemType.Equipment:
                        UpdateEquipmentInformation(item, m_items.Where(g => m_findGOFunc(g, item.Id)), heldQuantity);
                        break;
                    case ItemType.Consumable:
                        UpdateItemQuantity(item, m_items.FirstOrDefault(g => m_findGOFunc(g, item.Id)), heldQuantity);
                        break;
                }
            }

            if (m_items.Count > 0 && m_currentItemIndex > m_items.Count - 1)
            {
                m_currentItemIndex = m_items.Count - 1;
            }

            RefreshHierarchy();
            //SetNavigation();
        }

        private void UpdateItemQuantity(Item item, GameObject guiItem, int quantity, PlayableCharacter owner = null, Slot slot = Slot.None)
        {
            if (guiItem != null && quantity <= 0)
            {
                DestroyItem(guiItem);
            }
            else if (guiItem != null && quantity > 0)
            {
                guiItem.GetComponent<UI_SubMenu_Inventory_Item>().Refresh(quantity);
            }
            else if (guiItem == null && quantity > 0)
            {
                InstantiateItem(item, quantity);
            }
        }

        // Update the content of an instantiate Equipment item
        private void UpdateEquipmentInformation(Item item, IEnumerable<GameObject> guiItems, int quantity)
        {
            // Retrieve owners of equipment
            var equippedItems = GetEquipmentOwner(item.Id);
            List<GameObject> deletionQueue = new List<GameObject>();

            //Refresh quantity of potential unequipped equipment.
            UpdateItemQuantity(item, guiItems.SingleOrDefault(g => g.GetComponent<UI_SubMenu_Inventory_Item>().GetOwner() == null), quantity - equippedItems.Values.Sum(x => x.Count()));

            //Remove unequipped items
            foreach (var guiItem in guiItems.Where(g => g.GetComponent<UI_SubMenu_Inventory_Item>().GetOwner() != null))
            {
                var i = guiItem.GetComponent<UI_SubMenu_Inventory_Item>();
                if (!equippedItems.ContainsKey(i.GetOwner()) || !equippedItems.Values.Any(slots => slots.Contains(i.GetSlot())))
                {
                    deletionQueue.Add(guiItem);
                }
            }

            //Add equipped items
            foreach (var equipedItem in equippedItems)
            {
                var charGuiItems = guiItems.Where(g => g.GetComponent<UI_SubMenu_Inventory_Item>().GetOwner() == equipedItem.Key);
                foreach (Slot slot in equipedItem.Value)
                {
                    if (!charGuiItems.Any(g => slot == g.GetComponent<UI_SubMenu_Inventory_Item>().GetSlot()))
                    {
                        InstantiateItem(item, 1, equipedItem.Key);
                    }
                }
            }
            deletionQueue.ForEach(x => DestroyItem(x));
        }


        private Dictionary<PlayableCharacter, IEnumerable<Slot>> GetEquipmentOwner(string id)
        {
            return m_partyManager.GetAllExistingPartyMembers().Where(p => p.EquipmentSlots.IsEquiped(id)).ToDictionary(p => p, p => p.EquipmentSlots.GetEquipedSlot(id));
        }


        private void RefreshHierarchy()
        {
            m_items = m_items.OrderBy(x => x.name).ToList();

            for (int i = 0; i < m_items.Count - 1; i++)
            {
                m_items[i].transform.SetSiblingIndex(i);
            }
        }

        private void SetNavigation()
        {
            foreach (var item in m_items)
            {
                var index = m_items.IndexOf(item);

                item.GetComponent<Button>().ExplicitNavigation(
                    Up: index > 0 ? m_items[index - 1].GetComponent<Button>() : null,
                    Down: index < m_items.Count - 1 ? m_items[index + 1].GetComponent<Button>() : null);

            }
        }

        private GameObject InstantiateItem(Item item, int count, PlayableCharacter owner = null, Slot slot = Slot.None)
        {
            var uiItem = Instantiate(ItemGo);
            uiItem.transform.SetParent(ItemList.transform);
            uiItem.name = item.Id;
            uiItem.transform.localScale = new Vector3(1, 1, 1);

            uiItem.GetComponent<UI_SubMenu_Inventory_Item>().Initialize(item, count);
            if (owner != null)
            {
                uiItem.name += $"_{owner.Id}_{slot}";
                uiItem.GetComponent<UI_SubMenu_Inventory_Item>().SetOWner(owner, slot);
            }
            m_items.Add(uiItem);
            return uiItem;
        }

        /// <summary>
        /// Destroy display item and remove it from the list.
        /// </summary>
        /// <param name="obj"></param>
        private void DestroyItem(GameObject obj)
        {
            Destroy(obj);
            m_items.Remove(obj);
        }
    }
}
