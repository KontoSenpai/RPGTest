using System;
using UnityEngine;
using RPGTest.Helpers;
using RPGTest.Inputs;
using RPGTest.UI.Widgets;
using RPGTest.Models.Items;
using RPGTest.Models.Entity;
using RPGTest.Enums;
using System.Linq;

namespace RPGTest.UI.InventoryMenu
{
    public class UI_SubMenu_Inventory_Equipment : MonoBehaviour
    {
        [SerializeField]
        private UI_EquipmentSlot[] m_equipmentSlots;

        [HideInInspector]
        public CancelActionHandler ItemInteractionCancelled { get; set; }
        [HideInInspector]
        public EquipmentSlotSelectedHandler SlotSelected { get; set; }

        private PlayableCharacter m_character;
        private Equipment m_selectedItem;

        private Controls m_playerInput;

        public virtual void Awake()
        {
            m_playerInput = new Controls();
            m_playerInput.UI.Navigate.performed += Navigate_performed;
            m_playerInput.UI.Cancel.performed += Cancel_performed;

            foreach (UI_EquipmentSlot slot in m_equipmentSlots)
            {
                slot.SlotSelected += OnSlotSelected;
            }
        }

        public void OnEnable() => m_playerInput.Enable();
        public void OnDisable() => m_playerInput.Disable();

        #region Input Events
        private void Cancel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.ItemInteractionCancelled();
        }

        private void Navigate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var movement = obj.ReadValue<Vector2>();
            if (movement.x < 0)
            {
                //TryChangePurchaseQuantity(-1);
            }
            else if (movement.x > 0)
            {
                //TryChangePurchaseQuantity(1);
            }
        }
        #endregion


        #region Events
        public void OnSlotSelected()
        {

        }

        public void OnSlotDeselected()
        {
        }
        #endregion

        #region Public Methods
        public void Initialize(Equipment item)
        {
            m_selectedItem = item;
        }

        public void Focus()
        {
            if (m_selectedItem.EquipmentType < EquipmentType.Helmet) // Weapons
            {
                m_equipmentSlots.ForEach(s => s.Enable(s.Slot < Slot.Head));
            } 
            else if (m_selectedItem.EquipmentType < EquipmentType.HeavyArmor) // Heads
            {
                m_equipmentSlots.ForEach(s => s.Enable(s.Slot < Slot.Body));
            } 
            else if (m_selectedItem.EquipmentType < EquipmentType.Accessory) // Armor
            {
                m_equipmentSlots.ForEach(s => s.Enable(s.Slot < Slot.Accessory1));
            } else // Accessories
            {
                m_equipmentSlots.ForEach(s => s.Enable(s.Slot > Slot.Body));
            }

            foreach (var slot in m_equipmentSlots)
            {
                var index = Array.IndexOf(m_equipmentSlots, slot);

                if (index % 2 == 0) // Even = left slot
                {
                    slot.GetButton().ExplicitNavigation(
                        Right : index < m_equipmentSlots.Length - 1 && m_equipmentSlots[index + 1].GetButton().interactable ? m_equipmentSlots[index + 1].GetButton() : null,
                        Up: index > 1 && m_equipmentSlots[index - 2].GetButton().interactable ? m_equipmentSlots[index - 2].GetButton() : null,
                        Down: index < m_equipmentSlots.Length - 2 && m_equipmentSlots[index + 2].GetButton().interactable ? m_equipmentSlots[index + 2].GetButton() : null);
                } else // Odd = right slot
                {
                    slot.GetButton().ExplicitNavigation(
                        Left: index > 0 && m_equipmentSlots[index - 1].GetButton().interactable ? m_equipmentSlots[index - 1].GetButton() : null,
                        Up: index > 1 && m_equipmentSlots[index - 2].GetButton().interactable ? m_equipmentSlots[index - 2].GetButton() : null,
                        Down: index < m_equipmentSlots.Length - 2 && m_equipmentSlots[index + 2].GetButton().interactable ? m_equipmentSlots[index + 2].GetButton() : null);
                }
            }

            m_equipmentSlots.FirstOrDefault(s => s.GetButton().interactable).GetButton().Select();
        }

        public void UpdateContent(PlayableCharacter character)
        {
            m_character = character;
            foreach (UI_EquipmentSlot slot in m_equipmentSlots)
            {
                slot.Initialize(m_character.EquipmentSlots.Equipment[slot.Slot], m_selectedItem);
            }
        }

        public void Clean()
        {
            m_character = null;
        }
        #endregion

        #region Private Methods
        private void OnSlotSelected(Slot slot)
        {
            SlotSelected(slot);
        }
        #endregion
    }
}
