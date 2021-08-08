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
using UnityEngine.UI;

namespace RPGTest.UI.InventoryMenu
{
    public enum MenuItemActionType
    {
        Use,
        Equip,
        Unequip,
        Cancel,
        Throw
    }

    public class UI_SubMenu_Inventory : UI_Pause_SubMenu
    {
        [SerializeField] private GameObject ActionSelectionWindow;
        [SerializeField] private GameObject EquipWindow;
        [SerializeField] private GameObject ItemList;
        [SerializeField] private GameObject ItemInstantiate;
        public UI_SubMenu_Inventory_Details DetailsWidget;
        public UI_SubMenu_Inventory_Description DescriptionWidget;

        private List<GameObject> m_allGuiItems = new List<GameObject>();
        private Func<GameObject, string, bool> m_findGOFunc = (gameO, id) => gameO.GetComponent<UI_SubMenu_Inventory_Item>().GetItem().Id == id;
        InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;
        PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        private int m_currentNavigationIndex = 0;
        private bool m_actionInProgress = false;

        public override void Awake()
        {
            base.Awake();
            m_playerInput.UI.Navigate.performed += Navigate_performed;
        }

        private void Navigate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (IsSubMenuSelected && !m_actionInProgress)
            {
                var movement = obj.ReadValue<Vector2>();
                if (movement.y > 0 && m_currentNavigationIndex > 0)
                {
                    m_currentNavigationIndex -= 1;
                }
                else if (movement.y < 0 && m_currentNavigationIndex < m_allGuiItems.Count - 1)
                {
                    m_currentNavigationIndex += 1;
                }
                RefreshDetailsPanel();
            }
        }

        public override void OpenMenu()
        {
            base.OpenMenu();
            if (m_allGuiItems.Count > 0)
            {
                m_allGuiItems[0].GetComponent<Button>().Select();
            }
            DetailsWidget.SetVisible(true);
            DescriptionWidget.SetVisible(true);
            RefreshDetailsPanel();
            ActionSelectionWindow.GetComponent<UI_SubMenu_Inventory_ActionSelection>().ItemActionSelected += ItemAction_Selected;
        }

        public override void CloseMenu()
        {
            ActionSelectionWindow.GetComponent<UI_SubMenu_Inventory_ActionSelection>().ItemActionSelected -= ItemAction_Selected;
            base.CloseMenu();
        }

        public override void Initialize(bool refreshAll = true)
        {
            m_actionInProgress = false;
            m_allGuiItems = new List<GameObject>();

            if(refreshAll)
            {
                Clear();
            }

            Refresh();
        }

        public override void Clear()
        {
            m_allGuiItems.ForEach(x => Destroy(x));
            m_allGuiItems.Clear();
        }

        public void RefreshDetailsPanel()
        {
            var id = m_allGuiItems[m_currentNavigationIndex].GetComponent<UI_SubMenu_Inventory_Item>().GetItem().Id;
            DetailsWidget.Refresh(ItemCollector.TryGetItem(id));
            DescriptionWidget.Refresh(ItemCollector.TryGetItem(id));
        }


        #region Private Methods
        private GameObject InstantiateItem(Item item, int count, PlayableCharacter owner = null, Slot slot = Slot.None)
        {
            var uiItem = Instantiate(ItemInstantiate);
            uiItem.transform.SetParent(ItemList.transform);
            uiItem.name = item.Id;
            uiItem.transform.localScale = new Vector3(1, 1, 1);

            int heldQuanity = m_inventoryManager.GetHeldItemQuantity(item.Id);
            uiItem.GetComponent<UI_SubMenu_Inventory_Item>().Initialize(item, count);
            uiItem.GetComponent<UI_SubMenu_Inventory_Item>().ItemSelected += Item_Selected;
            if(owner != null && slot != Slot.None)
            {
                uiItem.name += $"_{owner.Id}_{slot}";
                uiItem.GetComponent<UI_SubMenu_Inventory_Item>().SetOWner(owner, slot);
            }
            m_allGuiItems.Add(uiItem);
            return uiItem;
        }

        private void Refresh(List<Item> items = null)
        {
            if(items == null)
            {
                items = m_inventoryManager.GetAllItems().Keys.Select(x => ItemCollector.TryGetItem(x)).ToList();
            }

            foreach (var item in items)
            {
                int heldQuantity = m_inventoryManager.GetHeldItemQuantity(item.Id);
                switch(item.Type)
                {
                    case ItemType.Equipment:
                        RefreshEquipment(item, m_allGuiItems.Where(g => m_findGOFunc(g, item.Id)), heldQuantity);
                        break;
                    case ItemType.Consumable:
                        RefreshItem(item, m_allGuiItems.FirstOrDefault(g => m_findGOFunc(g, item.Id)), heldQuantity);
                        break;
                }
            }
            if (m_allGuiItems.Count > 0 && m_currentNavigationIndex > m_allGuiItems.Count - 1)
            {
                m_currentNavigationIndex = m_allGuiItems.Count - 1;
            }

            RefreshHierarchy();
            SetNavigation();

        }

        private void RefreshItem(Item item, GameObject guiItem, int quantity, PlayableCharacter owner = null, Slot slot = Slot.None)
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


        private void RefreshEquipment(Item item, IEnumerable<GameObject> guiItems, int quantity)
        {
            var equipedItems = GetEquipedItems(item.Id);
            List<GameObject> deletionQueue = new List<GameObject>();
            //Refresh potential unequipped equipment.
            RefreshItem(item, guiItems.SingleOrDefault(g => g.GetComponent<UI_SubMenu_Inventory_Item>().GetOwner() == null), quantity - equipedItems.Values.Sum(x => x.Count()));

            //Remove unequipped items
            foreach(var guiItem in guiItems.Where(g => g.GetComponent<UI_SubMenu_Inventory_Item>().GetOwner() != null))
            {
                var i = guiItem.GetComponent<UI_SubMenu_Inventory_Item>();
                if (!equipedItems.ContainsKey(i.GetOwner()) || !equipedItems.Values.Any(slots => slots.Contains(i.GetSlot())))
                {
                    deletionQueue.Add(guiItem);
                }
            }

            //Add equipped items
            foreach(var equipedItem in equipedItems)
            {
                var charGuiItems = guiItems.Where(g => g.GetComponent<UI_SubMenu_Inventory_Item>().GetOwner() == equipedItem.Key);
                foreach (Slot slot in equipedItem.Value)
                {
                    if (!charGuiItems.Any(g => slot == g.GetComponent<UI_SubMenu_Inventory_Item>().GetSlot()))
                    {
                        InstantiateItem(item, 1, equipedItem.Key, slot);
                    }
                }
            }
            deletionQueue.ForEach(x => DestroyItem(x));
        }

        private void Focus()
        {
            m_allGuiItems = m_allGuiItems.WhereNotNull().ToList();
            if (m_allGuiItems.Count > 0 && m_currentNavigationIndex > m_allGuiItems.Count - 1)
            {
                m_currentNavigationIndex = m_allGuiItems.Count - 1;
            }
            ActionSelectionWindow.GetComponent<UI_SubMenu_Inventory_ActionSelection>().Close();
            m_actionInProgress = false;
            m_allGuiItems[m_currentNavigationIndex].GetComponent<Button>().Select();
            m_playerInput.UI.Cancel.performed += Cancel_performed;
        }

        private void SetNavigation()
        {
            foreach (var item in m_allGuiItems)
            {
                var index = m_allGuiItems.IndexOf(item);

                item.GetComponent<Button>().ExplicitNavigation(
                    Up: index > 0 ? m_allGuiItems[index - 1].GetComponent<Button>() : null,
                    Down: index < m_allGuiItems.Count - 1 ? m_allGuiItems[index + 1].GetComponent<Button>() : null);

            }
        }

        private Dictionary<PlayableCharacter, IEnumerable<Slot>> GetEquipedItems(string id)
        {
            return m_partyManager.GetAllExistingPartyMembers().Where(p => p.EquipmentSlots.IsEquiped(id)).ToDictionary(p => p, p => p.EquipmentSlots.GetEquipedSlot(id));
        }


        private void RefreshHierarchy()
        {
            GameObject currentItem = m_allGuiItems[m_currentNavigationIndex];

            m_allGuiItems = m_allGuiItems.OrderBy(x => x.name).ToList();

            for(int i = 0;  i < m_allGuiItems.Count - 1; i++)
            {
                m_allGuiItems[i].transform.SetSiblingIndex(i);
            }
        }

        /// <summary>
        /// Destroy display item and remove it from the list.
        /// </summary>
        /// <param name="obj"></param>
        private void DestroyItem(GameObject obj)
        {
            Destroy(obj);
            m_allGuiItems.Remove(obj);
        }
        #endregion

        #region events
        public void Item_Selected(Item selectedItem)
        {
            m_actionInProgress = true;
            ActionSelectionWindow.transform.position = m_allGuiItems[m_currentNavigationIndex].transform.position;
            ActionSelectionWindow.SetActive(true);
            switch(selectedItem.Type)
            {
                case ItemType.Consumable:
                    ActionSelectionWindow.GetComponent<UI_SubMenu_Inventory_ActionSelection>().InitializeUse(selectedItem);
                    break;
                case ItemType.Equipment:
                    var slot = m_allGuiItems[m_currentNavigationIndex].GetComponent<UI_SubMenu_Inventory_Item>().GetSlot();
                    if(slot == Slot.None)
                    {
                        ActionSelectionWindow.GetComponent<UI_SubMenu_Inventory_ActionSelection>().InitializeEquip(selectedItem);
                    }
                    else
                    {
                        ActionSelectionWindow.GetComponent<UI_SubMenu_Inventory_ActionSelection>().InitializeUnequip(selectedItem, slot, m_allGuiItems[m_currentNavigationIndex].GetComponent<UI_SubMenu_Inventory_Item>().GetOwner());
                    }

                    break;
            }
            m_playerInput.UI.Cancel.performed -= Cancel_performed;
        }

        public void ItemAction_Selected(MenuItemActionType actionType, List<Item> items)
        {
            switch(actionType)
            {
                case MenuItemActionType.Use:
                    Refresh(items);
                    break;
                case MenuItemActionType.Equip:
                case MenuItemActionType.Unequip:
                case MenuItemActionType.Throw:
                    Refresh(items);
                    Focus();
                    break;
                case MenuItemActionType.Cancel:
                    Refresh();
                    Focus();
                    break;
            }
        }
        #endregion
    }
}
