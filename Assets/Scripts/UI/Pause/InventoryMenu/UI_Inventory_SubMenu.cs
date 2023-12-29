using RPGTest.Enums;
using RPGTest.Managers;
using RPGTest.Models.Items;
using RPGTest.UI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace RPGTest.UI.InventoryMenu
{
    public class UI_Inventory_SubMenu : UI_Pause_SubMenu
    {
        [SerializeField] private UI_ItemList ItemList;
        [SerializeField] private UI_ItemListFilters ItemListFilters;
        [SerializeField] private UI_ItemListUpdator ItemListUpdator;

        [SerializeField] private UI_Item_Informations ItemInformationsPanel;

        [SerializeField] private UI_ActionConfirmationDialog ActionConfirmationDialog;

        [SerializeField] private UI_Inventory_UseItem ItemUsageWindow;
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

            ItemList.ItemSelectionChanged += OnItemSelection_Changed;

            ItemListFilters.FilterChanged += OnFilter_Changed;

            ItemUsageWindow.ItemInteractionCancelled += OnItemAction_Cancelled;
            ItemUsageWindow.ItemInteractionPerformed += OnItemAction_Performed;

            ItemQuantityDialog.DialogActionConfirmed += OnItemQuantityDialogAction_Confirmed;
            ItemQuantityDialog.DialogActionCancelled += OnItemQuantityDialogAction_Cancelled;

            ActionConfirmationDialog.ActionSelected += OnActionSelected_Confirmed;
        }

        public override void Initialize()
        {

        }

        public override void OpenSubMenu(Dictionary<string, object> parameters)
        {
            base.OpenSubMenu(parameters);

            InitializeInternal();

            UpdateInputActions();
        }

        public override void CloseSubMenu()
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
            ItemList.Close();
            base.CloseSubMenu();
        }

        public override void ExitPause()
        {
            if (ActionConfirmationDialog != null)
            {
                ActionConfirmationDialog.Close();
            }
            if (ItemUsageWindow != null)
            {
                ItemUsageWindow.Close();
            }

            base.ExitPause();
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
            ItemListFilters.CycleFilters();
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

            List<ItemUpdate> itemUpdates = new List<ItemUpdate>()
            {
                new ItemUpdate(m_pendingItemSelection.Item, m_inventoryManager.GetHeldItemQuantity(m_pendingItemSelection.Item.Id)),
            };

            var guiCDs = ItemListUpdator.UpdateItems(ItemList.GetItems().Select((i) => i.gameObject).ToList(), itemUpdates);
            ItemList.UpdateItems(guiCDs);
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

        /// <summary>
        /// Raised when a filter category change has been performed in <see cref="UI_ItemListFilters"/>
        /// Will send a query to a <see cref="UI_ItemList"/> to update hte items visibility
        /// </summary>
        /// <param name="filter">New Filter to control item visibility</param>
        public void OnFilter_Changed(ItemFilterCategory filter)
        {
            ItemList.ChangeItemsVisibility(filter, true);
        }
        
        /// <summary>
        /// Raised when item selection changed in the connected ItemList
        /// </summary>
        /// <param name="itemComponent"></param>
        public void OnItemSelection_Changed(GameObject itemGui)
        {
            if (itemGui != null) return;
            if (ItemInformationsPanel != null && itemGui.TryGetComponent<UI_InventoryItem>(out var component))
            {
                ItemInformationsPanel.Refresh(component.Item);
            }
        }

        /// <summary>
        /// Raised when an item has been selection has been confirmed in the item list
        /// </summary>
        /// <param name="sender">object who sent the event</param>
        /// <param name="e">EventArgs containing all necessary informations to process selection</param>
        public void OnItemSelection_Confirmed(object sender, ItemSelectionConfirmedEventArgs e)
        {
            m_pendingItemSelection = new PendingItemSelection()
            {
                Item = e.Item,

                Preset = e.Preset,
                Slot = e.Slot,
                Owner = e.Owner,
            };
            SelectItem();
        }

        public void OnItemAction_Performed(MenuActionType actionType, List<Item> items)
        {
            List<ItemUpdate> itemUpdates = new List<ItemUpdate>();
            foreach(var item in items)
            {
                itemUpdates.Add(new ItemUpdate(item, m_inventoryManager.GetHeldItemQuantity(item.Id)));
            }

            bool closeUsageWindow = false;
            var guiCDs = new List<GameObject>();
            switch (actionType)
            {
                case MenuActionType.Use:
                    guiCDs = ItemListUpdator.UpdateItems(ItemList.GetItems().Select((i) => i.gameObject).ToList(), itemUpdates);
                    closeUsageWindow = guiCDs.Count > 0;
                    break;
                case MenuActionType.Equip:
                case MenuActionType.Unequip:
                    guiCDs = ItemListUpdator.UpdateItems(ItemList.GetItems().Select((i) => i.gameObject).ToList(), itemUpdates);
                    closeUsageWindow = true;
                    break;
                case MenuActionType.Cancel:
                    closeUsageWindow = true;
                    break;
            }

            ItemList.UpdateItems(guiCDs);

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

        #region Private Methods
        private void InitializeInternal()
        {
            var items = ItemListUpdator.InstantiateItems(m_inventoryManager.GetItems());
            items.ForEach((i) =>
            {
                i.GetComponent<UI_InventoryItem>().ItemSelectionConfirmed += OnItemSelection_Confirmed;
            });

            ItemList.Initialize(items);
            ItemListFilters.Initialize();

            if (ItemInformationsPanel != null)
            {
                ItemInformationsPanel.SetVisible(true);
            }
        }

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
                    m_pendingItemSelection.Owner.EquipmentComponent.TryUnequip(m_pendingItemSelection.Preset, m_pendingItemSelection.Slot, out var removedEquipment);
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
            ExitPause();
        }
        #endregion
    }
}
