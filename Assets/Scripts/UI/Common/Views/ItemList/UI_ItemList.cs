using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Inputs;
using RPGTest.Models.Items;
using RPGTest.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    /// <summary>
    /// Component that manage the display of an item list.
    /// </summary>
    public class UI_ItemList : UI_View
    {
        [SerializeField] protected ScrollRect ItemList;
        [SerializeField] protected UI_ViewportBehavior ViewportBehaviour;
        [SerializeField] protected TextMeshProUGUI NoItemsDisplay;

        [HideInInspector]
        public ItemSelectionHandler ItemSelectionChanged { get; set; }

        private List<UI_InventoryItem> m_guiItems = new List<UI_InventoryItem>(); // List of available guiItems.

        private int m_selectedItemIndex = 0; // Keep track of selected item index to reset selection on refocus after usage

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

        public override Dictionary<string, string[]> GetInputDisplay(Controls playerInput = null)
        {
            var controls = playerInput ?? m_playerInput;
            m_inputActions = new Dictionary<string, string[]>()
            {
                {
                    "Select Item",
                    new string[]
                    {
                        "UI_" + controls.UI.Cycle.name
                    }
                },
                {
                    "Confirm",
                    new string[]
                    {
                        "UI_" + controls.UI.Submit.name,
                        "UI_" + controls.UI.LeftClick.name,
                    }
                },
            };

            return m_inputActions;
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

            m_guiItems = UI_List_Utils.RefreshHierarchy(ItemList.content.gameObject, m_guiItems.Select((i) => i.gameObject)).Select((g) => g.GetComponent<UI_InventoryItem>()).ToList();
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
                    Destroy(uiItem.gameObject);
                } else
                {
                    uiItem.gameObject.SetActive(true);
                    m_guiItems.Add(uiItem);
                }
            }

            m_guiItems = UI_List_Utils.RefreshHierarchy(ItemList.content.gameObject, m_guiItems.Select((i) => i.gameObject)).Select((g) => g.GetComponent<UI_InventoryItem>()).ToList();
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

        public void Close()
        {
            EventSystemEvents.OnSelectionUpdated -= OnSelection_Updated;
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
        /// <param name="select">Auto select first element of the list after new filter</param>
        public void ChangeItemsVisibility(ItemFilterCategory filter, bool select = false)
        {
            var visibleItems = new List<GameObject>();
            foreach (var item in m_guiItems)
            {
                if (SetItemVisibility(item, filter))
                {
                    visibleItems.Add(item.gameObject);
                }
            }
            if (select && visibleItems.Count > 0)
            {
                UI_List_Utils.SetVerticalNavigation(visibleItems);
                visibleItems[0].GetComponent<Button>().Select();
            }
            ViewportBehaviour.InitializeStepAmount(visibleItems.Count > 0 ? visibleItems[0] : null, visibleItems.Count);
        }

        public void ChangeItemsSelectability(bool isSelectable)
        {
            m_guiItems.ForEach((i) => i.GetComponent<Button>().interactable = isSelectable);
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
            if (currentSelection != null && currentSelection.transform.parent == ItemList.content)
            {
                ItemSelectionChanged(currentSelection);
                ViewportBehaviour.ScrollToSelection(currentSelection, previousSelection);
            }
            else if (currentSelection == null)
            {
                ItemSelectionChanged(currentSelection);
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
                case ItemFilterCategory.Head:
                    if (item.Type != ItemType.Equipment)
                    {
                        guiItem.gameObject.SetActive(false);
                    }
                    else
                    {
                        var equipment = (Equipment)item;
                        guiItem.gameObject.SetActive(equipment.EquipmentType >= EquipmentType.Helmet && equipment.EquipmentType <= EquipmentType.Hat);
                    }
                    break;
                case ItemFilterCategory.Body:
                    if (item.Type != ItemType.Equipment)
                    {
                        guiItem.gameObject.SetActive(false);
                    }
                    else
                    {
                        var equipment = (Equipment)item;
                        guiItem.gameObject.SetActive(equipment.EquipmentType >= EquipmentType.HeavyArmor && equipment.EquipmentType <= EquipmentType.LightArmor);
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
