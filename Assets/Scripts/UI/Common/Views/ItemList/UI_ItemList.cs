using RPGTest.Enums;
using RPGTest.Models.Items;
using RPGTest.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    public class UIItemDisplay
    {
        public Item Item { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Component that manage the display of an item list.
    /// </summary>
    public class UI_ItemList : MonoBehaviour
    {
        [SerializeField] protected GameObject ItemList;

        [HideInInspector]
        public event EventHandler<ItemSelectionConfirmedEventArgs> ItemSelectionConfirmed;

        [HideInInspector]
        public ItemSelectionChangedHandler ItemSelectionChanged { get; set; }
        [HideInInspector]
        public delegate void ItemSelectionChangedHandler(UI_InventoryItem item);

        private List<UI_InventoryItem> m_guiItems = new List<UI_InventoryItem>(); // List of available guiItems.

        private int m_selectedItemIndex = 0; // Keep track of selected item index to reset selection on refocus after usage

        // Lambda to retrieve an Item GameObject from an item declaration
        private Func<UI_InventoryItem, string, bool> m_FindInstantiatedGameObjectForItemID = (itemComponent, id) => itemComponent.Item.Id == id;

        #region Public Methods
        public void Awake() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        public void SelectDefault()
        {
            var obj = m_guiItems.FirstOrDefault((i) => i.gameObject.activeSelf);
            if (obj != null)
            {
                obj.GetComponent<Button>().Select();
                m_selectedItemIndex = m_guiItems.IndexOf(obj);
            } else
            {
                FindObjectOfType<EventSystem>().SetSelectedGameObject(null);
            }
        }

        public bool ReselectCurrentItem()
        {
            if (m_selectedItemIndex == -1 || m_guiItems[m_selectedItemIndex].gameObject.activeSelf == false)
                return false;

            m_guiItems[m_selectedItemIndex].GetComponent<Button>().Select();
            //FindObjectOfType<EventSystem>().SetSelectedGameObject(m_items[m_selectedItemIndex].gameObject);
            return true;
        }

        /// <summary>
        /// Place all instantiated guiItems in the component list.
        /// Then, 
        /// </summary>
        /// <param name="items"></param>
        public void Initialize(List<GameObject> items)
        {
            EventSystemEvents.OnSelectionUpdated += OnSelection_Updated;
            m_guiItems = items.Select((i) => i.GetComponent<UI_InventoryItem>()).ToList();

            UI_List_Utils.RefreshHierarchy(ItemList, m_guiItems.Select((i) => i.gameObject));
            UI_List_Utils.SetVerticalNavigation(m_guiItems.Select((i) => i.gameObject).ToList());
        }

        public void UpdateItems(List<GameObject> guiItems)
        {
            foreach(var item in guiItems)
            {
                var uiItem = item.GetComponent<UI_InventoryItem>();
                if (uiItem.Quantity == -1)
                {
                    m_guiItems.Remove(uiItem);
                    Destroy(uiItem);
                } else
                {
                    m_guiItems.Add(uiItem);
                }
            }

            UI_List_Utils.RefreshHierarchy(ItemList, m_guiItems.Select((i) => i.gameObject));
            UI_List_Utils.SetVerticalNavigation(m_guiItems.Select((i) => i.gameObject).ToList());
        }

        /// <summary>
        /// Delete all items and empty the list containing them
        /// </summary>
        public void Clear()
        {
            m_guiItems.ForEach(x => Destroy(x.gameObject));
            m_guiItems.Clear();
        }

        public List<UI_InventoryItem> GetItems()
        {
            return m_guiItems;
        }

        public UI_InventoryItem GetCurrentSelectedItem()
        {
            var go = FindObjectOfType<EventSystem>().currentSelectedGameObject;
            if (go != null && go.TryGetComponent<UI_InventoryItem>(out var inventoryItem))
            {
                return inventoryItem;
            }
            return null;
        }

        /// <summary>
        /// Will update visibility of all instantiated items in the list according to the given filter
        /// </summary>
        /// <param name="filter">Filter used to restrict which items should be visible</param>
        public void ChangeItemsVisibility(ItemFilterCategory filter)
        {
            var visibleItems = new List<GameObject>();
            foreach (var item in m_guiItems)
            {
                if (SetItemVisibility(item, filter))
                {
                    visibleItems.Add(item.gameObject);
                }
            }
            if (visibleItems.Count > 0)
            {
                UI_List_Utils.SetVerticalNavigation(visibleItems);
                visibleItems[0].GetComponent<Button>().Select();
            }
        }
        #endregion

        #region events
        /// <summary>
        /// Handled from the UIEventSystem, whenever a different <see cref="Selectable"/> changes
        /// </summary>
        /// <param name="currentSelection"></param>
        /// <param name="previousSelection"></param>
        public void OnSelection_Updated(GameObject currentSelection, GameObject previousSelection)
        {
            if (m_guiItems.Any((i) => i.gameObject == currentSelection))
            {
                ItemSelectionChanged(currentSelection.GetComponent<UI_InventoryItem>());
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Filter out which items should be visible depending on the selected filter
        /// </summary>
        /// <param name="guiItem"></param>
        /// <param name="filter"></param>
        private bool SetItemVisibility(UI_InventoryItem guiItem, ItemFilterCategory filter)
        {
            var item = guiItem.Item;
            switch (filter)
            {
                case ItemFilterCategory.All:
                    guiItem.gameObject.SetActive(true);
                    break;
                case ItemFilterCategory.Weapons:
                    if (item.Type != ItemType.Equipment)
                    {
                        guiItem.gameObject.SetActive(false);
                    } 
                    else
                    {
                        guiItem.gameObject.SetActive(((Equipment)item).EquipmentType < EquipmentType.Helmet);
                    }
                    break;
                case ItemFilterCategory.Armors:
                    if (item.Type != ItemType.Equipment)
                    {
                        guiItem.gameObject.SetActive(false);
                    }
                    else
                    {
                        var equipment = (Equipment)item;
                        guiItem.gameObject.SetActive(equipment.EquipmentType >= EquipmentType.Helmet && equipment.EquipmentType < EquipmentType.Accessory);
                    }
                    break;
                case ItemFilterCategory.Accessories:
                    if (item.Type != ItemType.Equipment)
                    {
                        guiItem.gameObject.SetActive(false);
                    }
                    else
                    {
                        guiItem.gameObject.SetActive(((Equipment)item).EquipmentType == EquipmentType.Accessory);
                    }
                    break;
                case ItemFilterCategory.Consumables:
                    guiItem.gameObject.SetActive(item.Type == ItemType.Consumable);
                    break;
                case ItemFilterCategory.Materials:
                    guiItem.gameObject.SetActive(item.Type == ItemType.Material);
                    break;
                case ItemFilterCategory.Valuable:
                    // guiItem.gameObject.SetActive(item.Type == ItemType.Material);
                    break;
                case ItemFilterCategory.Key:
                    guiItem.gameObject.SetActive(item.Type == ItemType.Key);
                    break;
                default:
                    throw new Exception("Unsupported item category");
            }

            return guiItem.gameObject.activeSelf;
        }
        
        private void SetIndexOfCurrentlySelectedItem(ItemSelectionConfirmedEventArgs args)
        {
            for (int i = 0; i < m_guiItems.Count(); i++)
            {
                var component = m_guiItems[i].GetComponent<UI_InventoryItem>();
                switch (args.Item.Type)
                {
                    case ItemType.Equipment:
                        if (component.Item == args.Item && component.GetOwner() == args.Owner && component.Slot == args.Slot)
                        {
                            m_selectedItemIndex = i;
                            return;
                        }
                        break;
                    default:
                        if (component.Item == args.Item)
                        {
                            m_selectedItemIndex = i;
                            return;
                        }
                        break;

                }
            }
            m_selectedItemIndex = 0;
        }
        #endregion
    }
}
