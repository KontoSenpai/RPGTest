using RPGTest.Enums;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using RPGTest.UI.Common;
using RPGTest.UI.Widgets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

    public class UI_Inventory_SubMenu : UI_Pause_SubMenu
    {
        [SerializeField] private UI_ItemList ItemList;
        [SerializeField] private UI_Inventory_UseItem ItemUsageWindow;

        public UI_SubMenu_Inventory_Details DetailsWidget;
        public UI_SubMenu_Inventory_Description DescriptionWidget;

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
                ItemList.OnNavigate_performed(ctx.ReadValue<Vector2>());
            };
            m_playerInput.UI.Navigate.canceled += ctx =>
            {
                m_navigateStarted = false;
            };
            m_playerInput.UI.Submit.performed += ctx =>
            {
                OnSubmit_Performed();
            };
            m_playerInput.UI.MouseMoved.performed += ctx =>
            {
                MouseMoved_Performed(ctx);
            };
            m_playerInput.UI.RightClick.performed += ctx =>
            {
                MouseMoved_Performed(ctx);
            };

            ItemUsageWindow.ItemInteractionCancelled += OnItemAction_Cancelled;
            ItemUsageWindow.ItemInteractionPerformed += OnItemAction_Performed;

            ItemList.ItemSelected += OnItem_Selected;
            ItemList.ItemUsed += OnItem_Used;

        }

        public override void OnEnable()
        {
            base.OnEnable();
            InputManager.SchemeChanged += OnScheme_Changed;
            UpdateControlsDisplay(GetInputActionDescriptions());
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (InputManager)
            {
                InputManager.SchemeChanged -= OnScheme_Changed;
            }
        }

        public override void OpenMenu(Dictionary<string, object> parameters)
        {
            base.OpenMenu(parameters);

            if(DetailsWidget != null)
            {
                DetailsWidget.SetVisible(true);
                RefreshDetailsPanel();
            }

            if(DescriptionWidget != null)
            {

                DescriptionWidget.SetVisible(true);
            }

            ItemList.SelectDefault();

            UpdateControlsDisplay(GetInputActionDescriptions());
            m_playerInput.UI.Cancel.performed += OnCancel_performed;
        }

        public override void CloseMenu()
        {
            base.CloseMenu();
            m_playerInput.UI.Cancel.performed -= OnCancel_performed;
        }

        public override void Initialize(bool refreshAll = true)
        {
            ItemList.Initialize();
        }

        public override void Clear()
        {
        }

        protected override Dictionary<string, string[]> GetInputActionDescriptions()
        {
            var actions = new Dictionary<string, string[]>()
            {
                {
                    "Change Item",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Navigate.name
                    }
                },
            };

            //if (m_actionInProgress)
            //{
                actions.Add("Validate Position",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Submit.name,
                        "UI_" + m_playerInput.UI.LeftClick.name
                    });
                actions.Add("Cancel Selection",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cancel.name
                    });
            //}
            //else
            //{
            //    actions.Add("Select Item",
            //        new string[]
            //        {
            //            "UI_" + m_playerInput.UI.Submit.name,
            //            "UI_" + m_playerInput.UI.LeftClick.name
            //        });
            //    actions.Add("Exit Menu",
            //        new string[]
            //        {
            //            "UI_" + m_playerInput.UI.Cancel.name
            //        });
            //}
            return actions;
        }

        public void RefreshDetailsPanel(Item item = null)
        {
            if (item == null)
            {
                item = ItemList.GetCurrentSelectedItem().GetItem();
            }

            DetailsWidget.Refresh(item);
            DescriptionWidget.Refresh(item);
        }

        #region Input Events
        public void OnSubmit_Performed()
        {
            var item = ItemList.GetCurrentSelectedItem();
            if (item == null) return;

            UseItem(item.GetItem(), item.GetSlot(), item.GetOwner());
        }

        protected override void OnCancel_performed(InputAction.CallbackContext ctx)
        {
            CancelCurrentAction();
        }

        // Deselect Button
        private void MouseMoved_Performed(InputAction.CallbackContext ctx)
        {
            FindObjectOfType<EventSystem>().SetSelectedGameObject(null);
            ItemList.OnMouseMoved_Performed(ctx);
        }

        public void MouseRightClick_Performed()
        {
            CancelCurrentAction();
        }
        #endregion

        #region events
        public void OnItem_Selected(Item item)
        {
            RefreshDetailsPanel(item);
        }

        protected override void OnScheme_Changed(object sender, EventArgs e)
        {
            UpdateControlsDisplay(GetInputActionDescriptions());
        }

        public void OnItem_Used(object sender, ItemUsedEventArgs e)
        {
            UseItem(e.Item, e.Slot, e.Owner);
        }

        public void OnItemAction_Performed(MenuItemActionType actionType, List<Item> items)
        {
            ItemUsageWindow.Close();

            switch (actionType)
            {
                case MenuItemActionType.Use:
                    ItemList.Refresh(false, items);
                    break;
                case MenuItemActionType.Equip:
                case MenuItemActionType.Unequip:
                case MenuItemActionType.Throw:
                    ItemList.Refresh(false, items);
                    break;
                case MenuItemActionType.Cancel:
                    ItemList.Refresh();
                    break;
            }

            ItemList.ReselectCurrentItem();
        }

        public void OnItemAction_Cancelled()
        {
            ItemUsageWindow.Close();
            ItemList.ReselectCurrentItem();
        }
        #endregion

        private void UseItem(Item item, Slot slot, PlayableCharacter owner)
        {
            m_playerInput.Disable();
            ItemUsageWindow.Initialize(item, slot, owner);
        }

        private void CancelCurrentAction()
        {
            CloseMenu();
        }
    }
}
