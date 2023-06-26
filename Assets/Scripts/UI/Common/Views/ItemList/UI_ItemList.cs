using RPGTest.Enums;
using RPGTest.Managers;
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
        [HideInInspector]
        public event EventHandler<ItemSelectionConfirmedEventArgs> ItemSelectionConfirmed;

        [SerializeField] protected GameObject FilterGo;
        [SerializeField] protected GameObject FilterList;

        [SerializeField] protected UI_ItemList_ItemInstantiator ItemInstantiator;
        [SerializeField] protected GameObject ItemList;

        private InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;
        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        protected List<UI_ItemCategoryFilter> m_filters = new List<UI_ItemCategoryFilter>(); // List of available guiItemFilters
        private List<UI_InventoryItem> m_guiItems = new List<UI_InventoryItem>(); // List of available guiItems.

        private ItemFilterCategory m_currentFilter = ItemFilterCategory.None; // Current item filter
        private int m_selectedItemIndex = 0; // Keep track of selected item index to reset selection on refocus after usage

        // Lambda to retrieve an Item GameObject from an item declaration
        private Func<UI_InventoryItem, string, bool> m_FindInstantiatedGameObjectForItemID = (itemComponent, id) => itemComponent.Item.Id == id;

        #region Public Methods
        public void Awake() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        public IEnumerable<UI_InventoryItem> GetItems()
        {
            return m_guiItems.Select(x => x.GetComponent<UI_InventoryItem>());
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
        /// Initialize the display information of each given gui item and filters.
        /// Clears both items and filters list before affectin the new objects
        /// </summary>
        /// <param name="items">Instantiated GUI items</param>
        /// <param name="availableFilters">Available filters</param>
        public virtual void Initialize(List<UIItemDisplay> itemDisplays, List<ItemFilterCategory> availableFilters)
        {
            InitializeItems(itemDisplays);
            InitializeFilters(availableFilters);
        }

        public void Clear()
        {
            m_guiItems.ForEach(x => Destroy(x.gameObject));
            m_guiItems.Clear();
            m_filters.ForEach((f) => Destroy(f.gameObject));
            m_filters.Clear();
        }

        public void Filter(ItemFilterCategory filter)
        {
            if (m_currentFilter == filter) return;

            FilterInternal(filter);
        }

        public void CycleFilters()
        {
            var index = Array.IndexOf(m_filters.ToArray(), m_filters.SingleOrDefault(f => f.GetFilter() == m_currentFilter));
            if (index != -1 && index == m_filters.Count -1)
            {
                Filter(m_filters[0].GetFilter());
            } else if (index != -1)
            {
                Filter(m_filters[index + 1].GetFilter());
            }
        }

        public void SetItemIndex()
        {
            var go = GetCurrentSelectedItem();

            if (go != null)
            {
                m_selectedItemIndex = m_guiItems.IndexOf(go);
            } else
            {
                m_selectedItemIndex = 0;
            }
        }
        #endregion

        #region events
        public void OnItemSelection_Confirmed(object sender, ItemSelectionConfirmedEventArgs args)
        {
            SetIndexOfCurrentlySelectedItem(args);
            ItemSelectionConfirmed.Invoke(this, args);
        }
        #endregion

        #region private methods
        /// <summary>
        /// Populate the list with the provided item informations
        /// </summary>
        private void InitializeItems(List<UIItemDisplay> itemDisplays)
        {
            foreach (var itemDisplay in itemDisplays)
            {
                var guiItems = ItemInstantiator.InstantiateItem(itemDisplay);
                m_guiItems.AddRange(guiItems.Select((i) => i.GetComponent<UI_InventoryItem>()));
            }

            m_guiItems.ForEach((i) => i.ItemSelectionConfirmed += ItemSelectionConfirmed);

            UI_List_Utils.RefreshHierarchy(ItemList, m_guiItems.Select((i) => i.gameObject));
            UI_List_Utils.SetVerticalNavigation(m_guiItems.Select((i) => i.gameObject).ToList());
        }

        /// <summary>
        /// Populate the filter list with provided filter informations
        /// </summary>
        /// <param name="filters"></param>
        private void InitializeFilters(List<ItemFilterCategory> filters)
        {
            foreach (var filter in filters)
            {
                InstantiateFilter(filter);
            }
            FilterInternal(filters[0]);
        }

        private void FilterInternal(ItemFilterCategory filter)
        {
            m_currentFilter = filter;
            foreach (var filterComponent in m_filters)
            {
                filterComponent.GetComponent<Button>().interactable = filterComponent.GetFilter() != m_currentFilter;
            }

            var visibleItems = new List<GameObject>();
            foreach (var item in m_guiItems)
            {
                if (SetItemVisibility(item, m_currentFilter))
                {
                    visibleItems.Add(item.gameObject);
                }
            }
            UI_List_Utils.SetVerticalNavigation(visibleItems);
        }

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
                case ItemFilterCategory.Key:
                    guiItem.gameObject.SetActive(item.Type == ItemType.Key);
                    break;
                default:
                    throw new Exception("Unsupported item category");
            }

            return guiItem.gameObject.activeSelf;
        }

        private GameObject InstantiateFilter(ItemFilterCategory filter)
        {
            var guiFilter = Instantiate(FilterGo);
            guiFilter.transform.SetParent(FilterList.transform);
            guiFilter.name = filter.ToString();
            guiFilter.transform.localScale = new Vector3(1, 1, 1);

            var filterComponent = guiFilter.GetComponent<UI_ItemCategoryFilter>();
            filterComponent.Initialize(filter);

            filterComponent.gameObject.GetComponent<Button>().onClick.AddListener(() => Filter(filter));

            m_filters.Add(filterComponent);

            return guiFilter;
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

        /// <summary>
        /// Destroy display item and remove it from the list.
        /// </summary>
        /// <param name="obj"></param>
        protected void DestroyItem(UI_InventoryItem guiItem)
        {
            guiItem.ItemSelectionConfirmed -= OnItemSelection_Confirmed;
            Destroy(guiItem.gameObject);
            m_guiItems.Remove(guiItem);
        }
        #endregion
    }
}
