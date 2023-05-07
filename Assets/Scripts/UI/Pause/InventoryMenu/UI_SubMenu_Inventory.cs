using RPGTest.Collectors;
using RPGTest.Core.Assets.Scripts.UI.Widgets;
using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Managers;
using RPGTest.Models;
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
        [SerializeField] private UI_ItemList ItemList;

        [SerializeField] private GameObject ActionSelectionWindow;
        [SerializeField] private GameObject EquipWindow;
        public UI_SubMenu_Inventory_Details DetailsWidget;
        public UI_SubMenu_Inventory_Description DescriptionWidget;

        private Func<GameObject, string, bool> m_findGOFunc = (gameO, id) => gameO.GetComponent<UI_SubMenu_Inventory_Item>().GetItem().Id == id;
        InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;
        PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        private int m_currentNavigationIndex = 0;
        private bool m_actionInProgress = false;

        private Dictionary<string, string[]> m_actions => new Dictionary<string, string[]>()
        {
            { "Cycle Presets", new string[] {"Secondary Navigate.horizontal" } },
        };

        public override void Awake()
        {
            base.Awake();
            m_playerInput.UI.Navigate.performed += ItemList.Navigate_performed;
        }

        public override void OpenMenu(Dictionary<string, object> parameters)
        {
            base.OpenMenu(parameters);

            Refresh();

            if(DetailsWidget != null)
            {
                DetailsWidget.SetVisible(true);
                RefreshDetailsPanel();
            }

            if(DescriptionWidget != null)
            {

                DescriptionWidget.SetVisible(true);
            }

            ActionSelectionWindow.GetComponent<UI_SubMenu_Inventory_ActionSelection>().ItemActionSelected += ItemAction_Selected;
            UpdateControlsDisplay(m_actions);
        }

        public override void CloseMenu()
        {
            ActionSelectionWindow.GetComponent<UI_SubMenu_Inventory_ActionSelection>().ItemActionSelected -= ItemAction_Selected;
            base.CloseMenu();
        }

        public override void Initialize(bool refreshAll = true)
        {
            m_actionInProgress = false;
            ItemList.Initialize();
        }

        public void RefreshDetailsPanel()
        {
            var id = ItemList.GetCurrentSelectedItem().GetItem().Id;
            DetailsWidget.Refresh(ItemCollector.TryGetItem(id));
            DescriptionWidget.Refresh(ItemCollector.TryGetItem(id));
        }


        #region Private Methods
        private void Refresh()
        {
            foreach(var item in ItemList.GetItems())
            {
                item.ItemSelected += Item_Selected;
            }
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
        #endregion

        #region events
        public void Item_Selected(Item selectedItem, Slot slot)
        {
            m_actionInProgress = true;
            ActionSelectionWindow.transform.position = ItemList.transform.position;
            ActionSelectionWindow.SetActive(true);
            switch(selectedItem.Type)
            {
                case ItemType.Consumable:
                    ActionSelectionWindow.GetComponent<UI_SubMenu_Inventory_ActionSelection>().InitializeUse(selectedItem);
                    break;
                case ItemType.Equipment:
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
