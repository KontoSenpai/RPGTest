using MyBox;
using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Managers;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using RPGTest.UI.Common;
using RPGTest.UI.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace RPGTest.UI.InventoryMenu
{
    public class PendingItemSelection
    {
        public Item Item { get; set; }
        public PlayableCharacter Owner { get; set; }
        public PresetSlot Preset { get; set; }
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

        private List<ItemFilterCategory> m_filterCategories = new List<ItemFilterCategory>()
        {
            ItemFilterCategory.All,
            ItemFilterCategory.Consumables,
            ItemFilterCategory.Weapons,
            ItemFilterCategory.Armors,
            ItemFilterCategory.Accessories,
            ItemFilterCategory.Materials,
            ItemFilterCategory.Key
        };

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
                OnNavigate_performed();
            };

            m_playerInput.UI.SecondaryAction.performed += OnSecondaryAction_Performed;

            m_playerInput.UI.Navigate.canceled += ctx =>
            {
                m_navigateStarted = false;
            };
            m_playerInput.UI.Cancel.performed += ctx =>
            {
                OnCancel_performed(ctx);
            };
            m_playerInput.UI.MouseMoved.performed += OnMouseMoved_Performed;
            m_playerInput.UI.RightClick.performed += OnMouseRightClick_Performed;

            ItemUsageWindow.ItemInteractionCancelled += OnItemAction_Cancelled;
            ItemUsageWindow.ItemInteractionPerformed += OnItemAction_Performed;

            ItemList.ItemSelectionConfirmed += OnItemSelection_Confirmed;

            ItemQuantityDialog.DialogActionConfirmed += OnItemQuantityDialogAction_Confirmed;
            ItemQuantityDialog.DialogActionCancelled += OnItemQuantityDialogAction_Cancelled;

            ActionConfirmationDialog.ActionSelected += OnActionSelected_Confirmed;
        }

        public override void Open(Dictionary<string, object> parameters)
        {
            base.Open(parameters);

            Initialize();

            if (ItemInformationsPanel != null)
            {    
                ItemInformationsPanel.SetVisible(true);
                RefreshInformationPanel();
            }
            UpdateInputActions();
            EventSystemEvents.OnSelectionUpdated += OnSelection_Updated;
        }

        public override void Close()
        {
            if (ActionConfirmationDialog != null)
            {
                ActionConfirmationDialog.Close();
            }
            if (ItemUsageWindow != null)
            {
                ItemUsageWindow.Close();
            }

            ItemList.Clear();
            base.Close();
        }

        public override void CloseMenu()
        {
            if (ActionConfirmationDialog != null)
            {
                ActionConfirmationDialog.Close();
            }
            if (ItemUsageWindow != null)
            {
                ItemUsageWindow.Close();
            }

            EventSystemEvents.OnSelectionUpdated += OnSelection_Updated;
            base.CloseMenu();
        }

        private void Initialize()
        {
            var displayItems = new List<UIItemDisplay>();

            foreach(var item in m_inventoryManager.GetItems())
            {
                var itemModel = ItemCollector.TryGetItem(item.Key);
                displayItems.Add(
                    new UIItemDisplay
                    {
                        Item = itemModel,
                        Quantity = m_inventoryManager.GetHeldItemQuantity(item.Key)
                    });
            };
            ItemList.Initialize(displayItems, m_filterCategories);
            ItemList.SelectDefault();
        }

        protected override void UpdateInputActions()
        {
            m_inputActions = new Dictionary<string, string[]>()
            {
                {
                    "Select Slot",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Navigate.name  + ".vertical"
                    }
                },
                {
                    "Confirm",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Submit.name,
                        "UI_" + m_playerInput.UI.LeftClick.name
                    }
                },
                {
                    "Exit",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cancel.name
                    }
                },
                {
                    "Change Filter",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.SecondaryAction.name,
                    }
                }
            };
            base.UpdateInputActions();
        }

        public void RefreshInformationPanel(Item item = null)
        {
            if (item == null)
            {
                item = ItemList.GetCurrentSelectedItem().Item;
            }
            ItemInformationsPanel.Refresh(item);
        }

        #region Input Events
        private void OnNavigate_performed()
        {
            if (FindObjectOfType<EventSystem>().currentSelectedGameObject == null)
            {
                ItemList.SelectDefault();
            }
        }

        private void OnSecondaryAction_Performed(InputAction.CallbackContext ctx)
        {
            ItemList.SetItemIndex();
            ItemList.CycleFilters();
            if(!ItemList.ReselectCurrentItem())
            {
                ItemList.SelectDefault();
            }
            // TO DO (maybe): Store item informations instead of index, to re-select same item after filter.
        }

        protected override void OnCancel_performed(InputAction.CallbackContext ctx)
        {
            CancelCurrentAction();
        }

        private void OnMouseRightClick_Performed(InputAction.CallbackContext ctx)
        {
            //CancelCurrentAction();
        }

        private void OnMouseMoved_Performed(InputAction.CallbackContext ctx)
        {
            FindObjectOfType<EventSystem>().SetSelectedGameObject(null);
        }
        #endregion

        #region events
        private void OnItemQuantityDialogAction_Confirmed(object sender, EventArgs e)
        {
            m_inventoryManager.RemoveItem(m_pendingItemSelection.Item.Id, ((ItemQuantitySelectedEventArgs)e).Quantity);

            var displayItems = new List<UIItemDisplay>() {
                new UIItemDisplay
                {
                    Item = m_pendingItemSelection.Item,
                    Quantity = m_inventoryManager.GetHeldItemQuantity(m_pendingItemSelection.Item.Id)
                }
            };
            //ItemList.UpdateItems(displayItems);
            Refocus();
        }

        /// <summary>
        /// Handler of event raised when ItemQuantity Dialog is closed by cancellation
        /// </summary>
        private void OnItemQuantityDialogAction_Cancelled(object sender, EventArgs e)
        {
            Refocus();
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
                RefreshInformationPanel(compoment.Item);
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
            List<UIItemDisplay> displayItems = new List<UIItemDisplay>();
            foreach(var item in items)
            {
                m_inventoryManager.GetHeldItemQuantity(item.Id);
                displayItems.Add(new UIItemDisplay
                {
                    Item = item,
                    Quantity = m_inventoryManager.GetHeldItemQuantity(item.Id)
                });
            }

            bool closeUsageWindow = false;
            switch (actionType)
            {
                case MenuActionType.Use:
                    //closeUsageWindow = ItemList.UpdateItems(displayItems);
                    break;
                case MenuActionType.Equip:
                case MenuActionType.Unequip:
                    //closeUsageWindow = ItemList.UpdateItems(displayItems);
                    break;
                case MenuActionType.Cancel:
                    //ItemList.UpdateItems(displayItems);
                    break;
            }

            if (closeUsageWindow)
            {
                ItemUsageWindow.Close();
                EnableControls();
                ItemList.ReselectCurrentItem();
            }
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
                    ItemUsageWindow.Open(m_pendingItemSelection.Item, m_pendingItemSelection.Owner, m_pendingItemSelection.Preset, m_pendingItemSelection.Slot);
                    break;
                case MenuActionType.Unequip:
                    m_pendingItemSelection.Owner.TryUnequip(m_pendingItemSelection.Preset, m_pendingItemSelection.Slot, out var removedEquipment);
                    break;
                case MenuActionType.Discard:
                    ItemQuantityDialog.Open();
                    ItemQuantityDialog.Initialize(
                        m_pendingItemSelection.Item,
                        ItemQuantitySuperiorLimit.OWNED);
                    break;
                case MenuActionType.Cancel:
                    Refocus();
                    break;
            }
        }

        private void Refocus()
        {
            EnableControls();
            ItemList.ReselectCurrentItem();
            UpdateInputActions();
        }

        private void CancelCurrentAction()
        {
            CloseMenu();
        }
    }
}
