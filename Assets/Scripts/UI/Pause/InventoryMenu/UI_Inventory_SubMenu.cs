﻿using MyBox;
using RPGTest.Enums;
using RPGTest.Managers;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using RPGTest.UI.Common;
using RPGTest.UI.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGTest.UI.InventoryMenu
{
    public class PendingItemSelection
    {
        public Item Item { get; set; }
        public PlayableCharacter Owner { get; set; }
        public Slot Slot { get; set; }
    }

    public class UI_Inventory_SubMenu : UI_Pause_SubMenu
    {
        [SerializeField] private UI_ItemList ItemList;
        [SerializeField] private UI_Inventory_UseItem ItemUsageWindow;

        [Separator("Action Confirmation Dialogs")]
        [SerializeField] private UI_ActionConfirmationDialog ActionConfirmationDialog;


        [Separator("Action Confirmation Dialogs")]
        [SerializeField] private UI_Item_Informations ItemInformationsPanel;
        [SerializeField] private UI_Dialog_ItemQuantity ItemQuantityDialog;

        private PendingItemSelection m_pendingItemSelection;

        private InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;

        public override void Awake()
        {
            base.Awake();
            m_playerInput.UI.Navigate.started += ctx =>
            {
                    m_navigateStarted = true;
            };
            m_playerInput.UI.Navigate.performed += ctx =>
            {
                m_performTimeStamp = Time.time + 0.3f;
                ItemList.OnNavigate_performed();
            };
            m_playerInput.UI.Navigate.canceled += ctx =>
            {
                m_navigateStarted = false;
            };
            m_playerInput.UI.Cancel.performed += ctx =>
            {
                OnCancel_performed(ctx);
            };
            m_playerInput.UI.MouseMoved.performed += ctx =>
            {
                ItemList.OnMouseMoved_Performed(ctx);
            };
            m_playerInput.UI.RightClick.performed += ctx =>
            {
                OnMouseRightClick_Performed();
            };

            ItemUsageWindow.ItemInteractionCancelled += OnItemAction_Cancelled;
            ItemUsageWindow.ItemInteractionPerformed += OnItemAction_Performed;

            ItemList.ItemSelected += OnItem_Selected;
            ItemList.ItemSelectionConfirmed += OnItemSelection_Confirmed;

            ItemQuantityDialog.DialogActionConfirmed += OnItemQuantityDialogAction_Confirmed;
            ItemQuantityDialog.DialogActionCancelled += OnItemQuantityDialogAction_Cancelled;

            ActionConfirmationDialog.ActionSelected += OnActionSelected_Confirmed;
        }

        public override void OpenMenu(Dictionary<string, object> parameters)
        {
            base.OpenMenu(parameters);
            ItemList.SelectDefault();

            if (ItemInformationsPanel != null)
            {    
                ItemInformationsPanel.SetVisible(true);
                RefreshInformationPanel();
            }
            UpdateInputActions();
            EventSystemEvents.OnSelectionUpdated += OnSelection_Updated;
        }

        public override void CloseMenu()
        {
            EventSystemEvents.OnSelectionUpdated += OnSelection_Updated;
            base.CloseMenu();

        }

        public override void Initialize(bool refreshAll = true)
        {
            ItemList.Initialize();
        }

        protected override void UpdateInputActions()
        {
            m_inputActions = new Dictionary<string, string[]>()
            {
                {
                    "Change Item",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Navigate.name  + ".vertical"
                    }
                },
                {
                    "Select Item",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Submit.name,
                        "UI_" + m_playerInput.UI.LeftClick.name
                    }
                },
                {
                    "Cancel Selection",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cancel.name
                    }
                }
            };
            base.UpdateInputActions();
        }

        public void RefreshInformationPanel(Item item = null)
        {
            if (item == null)
            {
                item = ItemList.GetCurrentSelectedItem().GetItem();
            }
            ItemInformationsPanel.Refresh(item);
        }

        #region Input Events
        protected override void OnCancel_performed(InputAction.CallbackContext ctx)
        {
            CancelCurrentAction();
        }

        public void OnMouseRightClick_Performed()
        {
            CancelCurrentAction();
        }
        #endregion

        #region events
        private void OnItemQuantityDialogAction_Confirmed(object sender, EventArgs e)
        {
            m_inventoryManager.RemoveItem(m_pendingItemSelection.Item.Id, ((ItemQuantitySelectedEventArgs)e).Quantity);
            ItemList.Refresh(false, new List<Item> { m_pendingItemSelection.Item });

            CloseItemQuantityDialog();
        }

        /// <summary>
        /// Handler of event raised when ItemQuantity Dialog is closed by cancellation
        /// </summary>
        private void OnItemQuantityDialogAction_Cancelled(object sender, EventArgs e)
        {
            CloseItemQuantityDialog();
        }

        public void OnActionSelected_Confirmed(MenuActionType actionType)
        {
            ExecuteAction(actionType);
        }

        public void OnActionSelection_Cancel()
        {
            EnableControls();
        }

        public void OnItem_Selected(Item item)
        {
            RefreshInformationPanel(item);
        }

        private void OnSelection_Updated(GameObject currentSelection, GameObject previousSelection)
        {
            if (currentSelection != null && currentSelection.TryGetComponent<UI_InventoryItem>(out var compoment))
            {
                RefreshInformationPanel(compoment.GetItem());
            }
        }

        /// <summary>
        /// Raised when an item has been selected in the item list
        /// </summary>
        /// <param name="sender">object who sent the event</param>
        /// <param name="e">EventArgs containing all necessary informations to process selection</param>
        public void OnItemSelection_Confirmed(object sender, ItemSelectionConfirmedEventArgs e)
        {
            m_pendingItemSelection = new PendingItemSelection()
            {
                Item = e.Item,
                Owner = e.Owner,
                Slot = e.Slot,
            };
            SelectItem();
        }

        public void OnItemAction_Performed(MenuActionType actionType, List<Item> items)
        {
            switch (actionType)
            {
                case MenuActionType.Use:
                    if(!ItemList.Refresh(false, items))
                    {
                        return;
                    }
                    break;
                case MenuActionType.Equip:
                case MenuActionType.Unequip:
                    ItemList.Refresh(false, items);
                    break;
                case MenuActionType.Cancel:
                    ItemList.Refresh();
                    break;
            }

            ItemUsageWindow.Close();
            EnableControls();
            ItemList.ReselectCurrentItem();
        }

        public void OnItemAction_Cancelled()
        {
            ItemUsageWindow.Close();

            EnableControls();
            ItemList.ReselectCurrentItem();
        }
        #endregion

        private void SelectItem()
        {
            switch (m_pendingItemSelection.Item.Type)
            {
                case ItemType.Consumable:
                    ActionConfirmationDialog.Open(new List<MenuActionType>() { MenuActionType.Use, MenuActionType.Discard });
                    break;
                case ItemType.Equipment:
                    var actions = new List<MenuActionType>();
                    if (m_pendingItemSelection.Owner != null)
                    {
                        actions.Add(MenuActionType.Unequip);
                    }
                    actions.Add(MenuActionType.Equip);
                    actions.Add(MenuActionType.Discard);
                    ActionConfirmationDialog.Open(actions);
                    break;
                case ItemType.Material:
                    ActionConfirmationDialog.Open(new List<MenuActionType>() { MenuActionType.Discard });
                    break;
            }
            DisableControls();
        }

        private void ExecuteAction(MenuActionType actionType)
        {
            switch (actionType)
            {
                case MenuActionType.Use:
                    ItemUsageWindow.Open(m_pendingItemSelection.Item);
                    break;
                case MenuActionType.Equip:
                    ItemUsageWindow.Open(m_pendingItemSelection.Item, m_pendingItemSelection.Owner, m_pendingItemSelection.Slot);
                    break;
                case MenuActionType.Unequip:
                    m_pendingItemSelection.Owner.TryUnequip(m_pendingItemSelection.Slot, out var removedEquipment);
                    break;
                case MenuActionType.Discard:
                    ItemQuantityDialog.Open();
                    ItemQuantityDialog.Initialize(
                        m_pendingItemSelection.Item,
                        ItemQuantitySuperiorLimit.OWNED);
                    break;
                case MenuActionType.Cancel:
                    ItemList.ReselectCurrentItem();
                    break;
            }
        }

        private void CloseItemQuantityDialog()
        {
            ItemQuantityDialog.Close();
            EnableControls();
            ItemList.ReselectCurrentItem();
        }

        private void CancelCurrentAction()
        {
            CloseMenu();
        }
    }
}
