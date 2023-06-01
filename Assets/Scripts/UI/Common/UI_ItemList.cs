using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Managers;
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
    /// <summary>
    /// Component that manage the display of an item list.
    /// </summary>
    public class UI_ItemList : MonoBehaviour
    {
        [SerializeField] private GameObject ItemGo;
        [SerializeField] private GameObject ItemList;

        private InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;
        PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        private List<GameObject> m_items = new List<GameObject>(); // List of instantiaced Button to represent an item.

        private int m_selectedItemIndex = 0; // Keep track of selected item index to reset selection on refocus after usage

        // Lambda to retrieve an Item GameObject from an item declaration
        private Func<GameObject, string, bool> m_findGOFunc = (gameO, id) => gameO.GetComponent<UI_InventoryItem>().GetItem().Id == id;

        [HideInInspector]
        public ItemSelectedHandler ItemSelected { get; set; }
        [HideInInspector]
        public delegate void ItemSelectedHandler(Item go);

        [HideInInspector]
        public event EventHandler<ItemSelectionConfirmedEventArgs> ItemSelectionConfirmed;

        #region input events
        public void OnNavigate_performed()
        {
            var go = FindObjectOfType<EventSystem>().currentSelectedGameObject;
            if (go == null)
            {
                FindObjectOfType<EventSystem>().SetSelectedGameObject(m_items[0]);
                go = m_items[0];
            }
            ItemSelected(go.GetComponent<UI_InventoryItem>().GetItem());
        }

        public void OnMouseMoved_Performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            FindObjectOfType<EventSystem>().SetSelectedGameObject(null);
        }
        #endregion

        #region events
        public void OnItemSelection_Confirmed(object sender, ItemSelectionConfirmedEventArgs args)
        {
            SetIndexOfCurrentlySelectedItem(args);
            ItemSelectionConfirmed.Invoke(this, args);
        }
        #endregion

        #region Public Methods
        public void SelectDefault()
        {
            m_items.First().GetComponent<Button>().Select();
            ItemSelected(m_items.First().GetComponent<UI_InventoryItem>().GetItem());
        }

        public void ReselectCurrentItem()
        {
            FindObjectOfType<EventSystem>().SetSelectedGameObject(m_items[m_selectedItemIndex]);
        }

        public void Initialize()
        {
            Refresh();
        }

        public bool Refresh(bool refreshAll = true, List<Item> items = null)
        {
            if (refreshAll)
            {
                Clear();
            }

            return RefreshItems(items);
        }

        public void Clear()
        {
            m_items.ForEach(x => Destroy(x));
            m_items.Clear();
        }

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
        #endregion

        private bool RefreshItems(List<Item> items = null)
        {
            bool refocus = false;
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
                        refocus = true;
                        break;
                    case ItemType.Consumable:
                        refocus = UpdateItemQuantity(item, m_items.FirstOrDefault(g => m_findGOFunc(g, item.Id)), heldQuantity);
                        break;
                }
            }

            if(m_items.Count > 0 && m_selectedItemIndex > m_items.Count -1)
            {
                m_selectedItemIndex = m_items.Count - 1;
            }

            RefreshHierarchy();
            SetNavigation();
            return refocus;
        }

        /// <summary>
        /// Refresh the display of item based on its quantity.
        /// If item existed new quantity is equal to 0, the instantiated button is deleted
        /// If item existed and new quantity is still 0, the insantiated button is updated
        /// If item didn't exist, a new button is instantiated
        /// </summary>
        /// <param name="item">Item to refresh</param>
        /// <param name="guiItem">GUI instantiation of item</param>
        /// <param name="quantity">Current quantity of item held</param>
        /// <param name="owner">(optional)Equipment Specific. To which character item got equipped</param>
        /// <param name="slot">(optional)Equipment Specific. On which slot item got equipped</param>
        /// <returns></returns>
        private bool UpdateItemQuantity(Item item, GameObject guiItem, int quantity, PlayableCharacter owner = null, Slot slot = Slot.None)
        {
            if (guiItem != null && quantity <= 0)
            {
                DestroyItem(guiItem);
                return true;
            }
            else if (guiItem != null && quantity > 0)
            {
                guiItem.GetComponent<UI_InventoryItem>().Refresh(quantity);
            }
            else if (guiItem == null && quantity > 0)
            {
                InstantiateItem(item, quantity);
            }
            return false;
        }

        // Update the content of an instantiate Equipment item
        private void UpdateEquipmentInformation(Item item, IEnumerable<GameObject> guiItems, int quantity)
        {
            // Retrieve owners of equipment
            var equippedItems = GetEquipmentOwner(item.Id);
            List<GameObject> deletionQueue = new List<GameObject>();

            //Refresh quantity of potential unequipped equipment.
            UpdateItemQuantity(item, guiItems.SingleOrDefault(g => g.GetComponent<UI_InventoryItem>().GetOwner() == null), quantity - equippedItems.Values.Sum(x => x.Count()));

            //Remove unequipped items
            foreach (var guiItem in guiItems.Where(g => g.GetComponent<UI_InventoryItem>().GetOwner() != null))
            {
                var i = guiItem.GetComponent<UI_InventoryItem>();
                if (!equippedItems.ContainsKey(i.GetOwner()) || !equippedItems.Values.Any(slots => slots.Contains(i.GetSlot())))
                {
                    deletionQueue.Add(guiItem);
                }
            }

            //Add equipped items
            foreach (var equipedItem in equippedItems)
            {
                var charGuiItems = guiItems.Where(g => g.GetComponent<UI_InventoryItem>().GetOwner() == equipedItem.Key);
                foreach (Slot slot in equipedItem.Value)
                {
                    if (!charGuiItems.Any(g => slot == g.GetComponent<UI_InventoryItem>().GetSlot()))
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

            uiItem.GetComponent<UI_InventoryItem>().Initialize(item, count);
            if (owner != null)
            {
                uiItem.name += $"_{owner.Id}_{slot}";
                uiItem.GetComponent<UI_InventoryItem>().SetOWner(owner, slot);
            }
            m_items.Add(uiItem);
            uiItem.GetComponent<UI_InventoryItem>().ItemSelectionConfirmed += OnItemSelection_Confirmed;
            return uiItem;
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
        /// Destroy display item and remove it from the list.
        /// </summary>
        /// <param name="obj"></param>
        private void DestroyItem(GameObject obj)
        {
            obj.GetComponent<UI_InventoryItem>().ItemSelectionConfirmed -= OnItemSelection_Confirmed;
            Destroy(obj);
            m_items.Remove(obj);
        }
    }
}
