﻿using MyBox;
using RPGTest.Inputs;
using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Managers;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.InventoryMenu
{
    public class UI_SubMenu_Inventory_ActionSelection : MonoBehaviour
    {
        [Separator("Buttons")]
        public Button ActionButton;
        public Button ThrowButton;
        public TextMeshProUGUI ActionString;

        [Separator("Sub Panels")]
        public GameObject UsePanel;
        public GameObject ThrowPanel;

        [Separator("UI To Hide")]
        public Image FormImage;

        [HideInInspector]
        public ItemUsedHandler ItemActionSelected { get; set; }
        [HideInInspector]
        public delegate void ItemUsedHandler(MenuItemActionType actionType, List<Item> items);

        private Item m_item;

        private PlayableCharacter m_owner;
        private Slot m_slot;

        private MenuItemActionType m_actionType;
        private ActionType m_action;
        
        private Controls m_playerInput;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;
        private InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;

        #region public Methods
        public virtual void Awake()
        {
            m_playerInput = new Controls();
            m_playerInput.UI.Cancel.performed += OnCancelPerformed;
        }


        public void OnEnable() => m_playerInput.Enable();
        public void OnDisable() => m_playerInput.Disable();

        /// <summary>
        /// Opens this UI component with the item we want to perform actions on
        /// </summary>
        /// <param name="item">Selected Item</param>
        public void InitializeUse(Item item)
        {
            m_item = item;

            ActionString.text = "Use";
            InitializeGui(MenuItemActionType.Use);
        }

        public void InitializeEquip(Item item)
        {
            m_item = item;

            ActionString.text = "Equip";
            InitializeGui(MenuItemActionType.Equip);
        }

        public void InitializeUnequip(Item item, Slot slot, PlayableCharacter owner)
        {
            m_item = item;
            m_slot = slot;
            m_owner = owner;

            ActionString.text = "Unequip";
            InitializeGui(MenuItemActionType.Unequip);
        }

        private void InitializeGui(MenuItemActionType type)
        {
            ActionButton.Select();
            m_actionType = type;

            if (m_actionType == MenuItemActionType.Equip || m_actionType == MenuItemActionType.Use)
            {
                var component = UsePanel.GetComponent<UI_SubMenu_Inventory_UseControl>();
                component.InitializePanel(type, m_item);
                component.ActionFinished += Action_Finished;

                if (!component.AnyApplicableMember)
                {
                    ActionButton.interactable = false;
                    var nav = ThrowButton.navigation;
                    nav.selectOnUp = null;
                    ThrowButton.navigation = nav;
                    ThrowButton.Select();
                }

            }
            else if (type == MenuItemActionType.Unequip)
            {
                m_action = ActionType.Equip;
            }

            UsePanel.SetActive(false);
            ThrowPanel.SetActive(false);

            ThrowPanel.GetComponent<UI_Item_Interaction>().ItemInteractionRequested += OnItemInteractionRequested;
        }

        /// <summary>
        /// Will initiate the closing of this UI component
        /// </summary>
        public void Close()
        {
            UsePanel.GetComponent<UI_SubMenu_Inventory_UseControl>().Clean();
            ThrowPanel.GetComponent<UI_Item_Interaction>().ItemInteractionRequested -= OnItemInteractionRequested;
            gameObject.SetActive(false);
        }

        #endregion


        #region private Methods
        private void HideUI(bool value)
        {
            this.ActionButton.gameObject.SetActive(value);
            this.ThrowButton.gameObject.SetActive(value);
            this.FormImage.enabled = value;

            if(value)
            {
                m_playerInput.UI.Cancel.performed += OnCancelPerformed;
            }
            else
            {
                m_playerInput.UI.Cancel.performed -= OnCancelPerformed;
            }
        }
        #endregion

        #region Events
        private void OnCancelPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            onClickCancel();
        }

        public void onClickOk()
        {
            switch(m_actionType)
            {
                case MenuItemActionType.Use:
                case MenuItemActionType.Equip:
                    UsePanel.SetActive(true);
                    var component = UsePanel.GetComponent<UI_SubMenu_Inventory_UseControl>();
                    component.Focus();
                    component.ItemInteractionCancelled += UseInteraction_Cancelled;
                    HideUI(false);
                    break;
                case MenuItemActionType.Unequip:
                    m_owner.TryUnequip(m_slot, out List<Item> removedItems);
                    ItemActionSelected(m_actionType, removedItems);
                    break;
            }

        }

        public void onClickThrow()
        {
            this.HideUI(false);

            ThrowPanel.SetActive(true);
            ThrowPanel.GetComponent<UI_Item_Interaction>().Initialize(InteractionType.Throw, m_item);
            ThrowPanel.GetComponent<UI_Item_Interaction>().ItemInteractionCancelled += ThrowInteraction_Cancelled;
            m_playerInput.UI.Cancel.performed -= OnCancelPerformed;
        }


        public void Action_Finished(MenuItemActionType actionType, List<Item> items)
        {
            ItemActionSelected(m_actionType, new List<Item> { m_item });
            if (m_inventoryManager.GetHeldItemQuantity(m_item.Id) == 0)
            {
                ItemActionSelected(MenuItemActionType.Cancel, null);
                Close();
            }
        }

        public void UseInteraction_Cancelled()
        {
            this.HideUI(true);

            var component = UsePanel.GetComponent<UI_SubMenu_Inventory_UseControl>();
            component.ItemInteractionCancelled += UseInteraction_Cancelled;
            UsePanel.SetActive(false);
            ActionButton.Select();
        }

        public void ThrowInteraction_Cancelled()
        {
            this.HideUI(true);

            ThrowPanel.GetComponent<UI_Item_Interaction>().ItemInteractionCancelled -= ThrowInteraction_Cancelled;
            ThrowPanel.SetActive(false);
            ThrowButton.Select();
        }

        public void onClickCancel()
        {
            ItemActionSelected(MenuItemActionType.Cancel, null);
        }

        public void OnItemInteractionRequested(Item item, int quantity)
        {
            m_inventoryManager.RemoveItem(item.Id, quantity);
            ItemActionSelected(MenuItemActionType.Throw, new List<Item>{ m_item });
            Close();
        }
        #endregion
    }
}
