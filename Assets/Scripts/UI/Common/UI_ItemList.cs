using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Managers;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
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

        [SerializeField] private GameObject FilterGo;
        [SerializeField] private GameObject FilterList;

        [SerializeField] private GameObject ItemGo;
        [SerializeField] private GameObject ItemList;

        [SerializeField] private bool StackOwnerOnSingleGuiItem;
        [SerializeField] private bool DisplayValue;

        private InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;
        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        private List<UI_ItemCategoryFilter> m_filters = new List<UI_ItemCategoryFilter>(); // List of available guiItemFilters
        private List<UI_InventoryItem> m_items = new List<UI_InventoryItem>(); // List of available guiItems.

        private ItemFilterCategory m_currentFilter; // Current item filter
        private int m_selectedItemIndex = 0; // Keep track of selected item index to reset selection on refocus after usage

        // Lambda to retrieve an Item GameObject from an item declaration
        private Func<UI_InventoryItem, string, bool> m_FindInstantiatedGameObjectForItemID = (itemComponent, id) => itemComponent.GetItem().Id == id;

        #region Public Methods
        public IEnumerable<UI_InventoryItem> GetItems()
        {
            return m_items.Select(x => x.GetComponent<UI_InventoryItem>());
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
            m_items.First().GetComponent<Button>().Select();
        }

        public void ReselectCurrentItem()
        {
            FindObjectOfType<EventSystem>().SetSelectedGameObject(m_items[m_selectedItemIndex].gameObject);
        }

        public void Initialize(List<UIItemDisplay> items, List<ItemFilterCategory> availableFilters)
        {
            m_items.ForEach((i) => Destroy(i));
            m_items.Clear();
            m_filters.ForEach((f) => Destroy(f));
            m_filters.Clear();

            InitializeItems(items);
            InitializeFilters(availableFilters);
        }

        public void Filter(ItemFilterCategory filter)
        {
            foreach(var item in m_items)
            {
                SetItemVisibility(item, filter);
            }
            SetNavigation();
        }

        public void Clear()
        {
            m_items.ForEach(x => Destroy(x));
            m_items.Clear();
        }

        /// <summary>
        /// Update the GuiItem corresponding to give items
        /// Will update the quantity, create or delete them based on the quantity passed
        /// </summary>
        /// <param name="items"></param>
        /// <returns>Whether or not to force closure of the use window</returns>
        public bool UpdateItems(List<UIItemDisplay> items)
        {
            bool focusList = false;

            foreach (var item in items)
            {
                switch (item.Item.Type)
                {
                    case ItemType.Equipment:
                        //UpdateEquipmentInformation(item, m_items.Where(g => m_FindInstantiatedGameObjectForItemID(g, item.Id)), heldQuantity);
                        focusList = true;
                        break;
                    case ItemType.Consumable:
                        focusList = UpdateItem(item);
                        break;
                }
            }

            if (m_items.Count > 0 && m_selectedItemIndex > m_items.Count - 1)
            {
                m_selectedItemIndex = m_items.Count - 1;
            }

            RefreshHierarchy();
            SetNavigation();

            return focusList;
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
        private void InitializeFilters(List<ItemFilterCategory> filters)
        {
            foreach(var filter in filters)
            {
                var guiFilter = InstantiateFilter(filter);
            }
        }

        /// <summary>
        /// Populate the list with the provided
        /// </summary>
        private void InitializeItems(List<UIItemDisplay> itemDisplays)
        {
            foreach (var itemDisplay in itemDisplays)
            {
                Debug.Log($"Item : {itemDisplay.Item.Name}");
                Debug.Log($"Total : {itemDisplay.Quantity}");
                switch (itemDisplay.Item.Type)
                {
                    case ItemType.Equipment:
                        UpdateEquipment(itemDisplay);
                        break;
                    default:
                        UpdateItem(itemDisplay);
                        break;
                }
            }

            RefreshHierarchy();
            SetNavigation();
        }

        /// <summary>
        /// Method to handle update of information of a given equipment piece.
        /// It will retrieve the potential owners, instantiate and destroy GameObjects as required
        /// </summary>
        /// <param name="itemDisplay">Informations of item to update</param>
        private void UpdateEquipment(UIItemDisplay itemDisplay)
        {
            // Retrieve potential owners of equipment
            var equippedItems = GetEquipmentOwners(itemDisplay.Item.Id);
            List<UI_InventoryItem> deletionQueue = new List<UI_InventoryItem>();

            var guiItems = m_items.Where((i) => i.GetItem().Id == itemDisplay.Item.Id);

            // Find Deleted GuiItems
            if (StackOwnerOnSingleGuiItem)
            {
                var owners = equippedItems.GroupBy((e) => e.Key).Select((e) => e.First().Key).ToList();
                var guiItem = guiItems.SingleOrDefault(g => g.GetComponent<UI_InventoryItem>().GetOwners() != null);
                // TODO : Logic for shop and grouped owners
            } 
            else
            {
                // Refresh quantity of potential unequipped equipment.
                var unequippedQuantity = GetTotalUnequippedQuantity(itemDisplay.Quantity, equippedItems);
                Debug.Log($"Unequipped : {unequippedQuantity}");
                if (guiItems.SingleOrDefault(g => g.GetOwner() == null) == null && unequippedQuantity > 0)
                {
                    InstantiateItem(itemDisplay.Item, unequippedQuantity);
                }
                else
                {
                    UpdateItemQuantity(guiItems.SingleOrDefault(g => g.GetOwner() == null), unequippedQuantity);
                }

                // Loop over existing GuiItems to find ones that need deletion
                foreach (var guiItem in guiItems.Where(g => g.GetOwner() != null))
                {
                    if (ShouldDeleteGuiItemForEquipment(guiItem, equippedItems))
                    {
                        deletionQueue.Add(guiItem);
                    }
                }

                // Loop over equipped items to find ones that need a new GuiItem
                foreach (var equippedItem in equippedItems)
                {
                    CreateGuiItemForEquipment(itemDisplay.Item, guiItems, equippedItem);
                }
            }

            // Destroy all guiItems that shouldn't be visible anymore
            deletionQueue.ForEach(x => DestroyItem(x));
        }

        /// <summary>
        /// Update a guiItem quantity if it exists, create it otherwise
        /// </summary>
        /// <param name="itemDisplay">Informations of item to update</param>
        private bool UpdateItem(UIItemDisplay itemDisplay)
        {
           var guiItem = m_items.FirstOrDefault(g => m_FindInstantiatedGameObjectForItemID(g, itemDisplay.Item.Id));
           if (guiItem == null)
           {
                InstantiateItem(itemDisplay.Item, itemDisplay.Quantity);
                return false;
           }

           return UpdateItemQuantity(guiItem, itemDisplay.Quantity);
        }

        /// <summary>
        /// Refresh the display of item based on its quantity.
        /// If item existed new quantity is equal to 0, the instantiated button is deleted
        /// If item existed and new quantity is still 0, the insantiated button is updated
        /// If item didn't exist, a new GuiItem is instantiated
        /// </summary>
        /// <param name="guiItem">GUI instantiation of item</param>
        /// <param name="quantity">Current quantity of item held</param>
        /// <returns></returns>
        private bool UpdateItemQuantity(UI_InventoryItem guiItem, int quantity)
        {
            if (guiItem == null) return false;

            if (quantity <= 0)
            {
                DestroyItem(guiItem);
                return true;
            }
            else
            {
                guiItem.GetComponent<UI_InventoryItem>().Refresh(quantity);
            }
            return false;
        }

        private Dictionary<PlayableCharacter, Dictionary<PresetSlot, IEnumerable<Slot>>> GetEquipmentOwners(string id)
        {
            return m_partyManager.GetAllExistingPartyMembers().Where(p => p.EquipmentSlots.IsEquiped(id)).ToDictionary(p => p, p => p.EquipmentSlots.GetEquipedSlots(id));
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

        /// <summary>
        /// Filter out which items should be visible depending on the selected filter
        /// </summary>
        /// <param name="guiItem"></param>
        /// <param name="filter"></param>
        private void SetItemVisibility(UI_InventoryItem guiItem, ItemFilterCategory filter)
        {
            var item = guiItem.GetItem();
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

        private GameObject InstantiateItem(Item item, int quantity)
        {
            var guiItem = Instantiate(ItemGo);
            guiItem.transform.SetParent(ItemList.transform);
            guiItem.name = item.Id;
            guiItem.transform.localScale = new Vector3(1, 1, 1);

            var inventoryItemComponent = guiItem.GetComponent<UI_InventoryItem>();
            inventoryItemComponent.Initialize(item, quantity);
            inventoryItemComponent.ItemSelectionConfirmed += OnItemSelection_Confirmed;

            m_items.Add(inventoryItemComponent);
            return guiItem;
        }

        private GameObject InstantiateEquipment(Item item, PlayableCharacter owner, PresetSlot preset, Slot slot)
        {
            var guiItem = InstantiateItem(item, 1);

            guiItem.name += $"_{owner.Id}";
            guiItem.GetComponent<UI_InventoryItem>().SetOwner(owner, preset, slot);

            return null;
        }

        private void SetIndexOfCurrentlySelectedItem(ItemSelectionConfirmedEventArgs args)
        {
            for (int i = 0; i < m_items.Count(); i++)
            {
                var item = m_items[i].GetComponent<UI_InventoryItem>();
                if (item.GetItem() == args.Item && item.GetOwner() == args.Owner && item.GetSlot() == args.Slot)
                {
                    m_selectedItemIndex = i;
                    return;
                }
            }
            m_selectedItemIndex = 0;
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
            foreach(var presetSlots in equippedSlots)
            {
                if (guiItem.GetPreset() == presetSlots.Key && presetSlots.Value.Contains(guiItem.GetSlot()))
                {
                    return false;
                }
            }
            return true;
        }

        private void CreateGuiItemForEquipment(Item item, IEnumerable<UI_InventoryItem> guiItems, KeyValuePair<PlayableCharacter, Dictionary<PresetSlot, IEnumerable<Slot>>> equippedItem)
        {
            Debug.Log($"Create for owner : {equippedItem.Key.Name}");
            var charGuiItems = guiItems.Where(g => g.GetOwner() == equippedItem.Key);
            foreach(var presetSlots in equippedItem.Value)
            {
                Debug.Log($"in preset : {presetSlots.Key}");
                foreach (var slot in presetSlots.Value)
                {
                    Debug.Log($"In slot : {slot}");
                    if (!charGuiItems.Any((g) => g.GetPreset() == presetSlots.Key && g.GetSlot() == slot))
                    {
                        InstantiateEquipment(item, equippedItem.Key, presetSlots.Key, slot);
                    }
                }
            }
        }

        private int GetTotalUnequippedQuantity(int quantity, Dictionary<PlayableCharacter, Dictionary<PresetSlot, IEnumerable<Slot>>> equippedItems)
        {
            var unequippedQuantity = quantity;
            foreach(var equippedItem in equippedItems)
            {
                foreach(var presetSlot in equippedItem.Value)
                {
                    unequippedQuantity -= presetSlot.Value.Count();
                }
            }
            return unequippedQuantity;
        }

        /// <summary>
        /// Destroy display item and remove it from the list.
        /// </summary>
        /// <param name="obj"></param>
        private void DestroyItem(UI_InventoryItem guiItem)
        {
            guiItem.ItemSelectionConfirmed -= OnItemSelection_Confirmed;
            Destroy(guiItem.gameObject);
            m_items.Remove(guiItem);
        }
        #endregion
    }
}
