using System;
using UnityEngine;
using RPGTest.Helpers;
using RPGTest.Inputs;
using RPGTest.UI.Widgets;
using RPGTest.Models.Items;
using RPGTest.Models.Entity;
using RPGTest.Enums;
using System.Linq;
using UnityEngine.EventSystems;

namespace RPGTest.UI.Common
{
    public class UI_EquipmentSlots : MonoBehaviour
    {
        [SerializeField]
        private UI_EquipmentSlot[] m_equipmentSlots;

        [HideInInspector]
        public CancelActionHandler EquipActionCancelled { get; set; }
        [HideInInspector]
        public EquipmentSlotSelectedHandler EquipActionPerformed { get; set; }

        private PlayableCharacter m_character;
        private Equipment m_selectedItem;

        private Controls m_playerInput;

        public virtual void Awake()
        {
            m_playerInput = new Controls();
            m_playerInput.UI.Submit.performed += OnSubmit_Performed;
            m_playerInput.UI.Navigate.performed += OnNavigate_performed;
            m_playerInput.UI.Cancel.performed += OnCancel_performed;

            foreach (UI_EquipmentSlot slot in m_equipmentSlots)
            {
                slot.SlotSelected += OnSlot_Selected;
            }
        }

        #region Input Events
        private void OnSubmit_Performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            var go = FindObjectOfType<EventSystem>().currentSelectedGameObject;
            if(go.TryGetComponent<UI_EquipmentSlot>(out var equipmentSlot))
            {
                equipmentSlot.onClick();
            }
        }

        private void OnCancel_performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            m_playerInput.Disable();
            EquipActionCancelled();
        }

        private void OnNavigate_performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            var movement = ctx.ReadValue<Vector2>();
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
        private void OnSlot_Selected(Slot slot)
        {
            m_playerInput.Disable();
            EquipActionPerformed(slot);
        }
        #endregion

        #region Public Methods
        public void Initialize(PlayableCharacter character, Equipment item)
        {
            m_character = character;
            m_selectedItem = item;

            foreach (UI_EquipmentSlot slot in m_equipmentSlots)
            {
                slot.Initialize(m_character.EquipmentSlots.Equipment[slot.Slot], m_selectedItem);
            }
        }

        public void Open()
        {
            m_playerInput.Enable();

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

            foreach (var equipmentSlot in m_equipmentSlots)
            {
                var index = Array.IndexOf(m_equipmentSlots, equipmentSlot);
                //slot.SlotSelected
                if (index % 2 == 0) // Even = left slot
                {
                    equipmentSlot.GetButton().ExplicitNavigation(
                        Right : index < m_equipmentSlots.Length - 1 && m_equipmentSlots[index + 1].GetButton().interactable ? m_equipmentSlots[index + 1].GetButton() : null,
                        Up: index > 1 && m_equipmentSlots[index - 2].GetButton().interactable ? m_equipmentSlots[index - 2].GetButton() : null,
                        Down: index < m_equipmentSlots.Length - 2 && m_equipmentSlots[index + 2].GetButton().interactable ? m_equipmentSlots[index + 2].GetButton() : null);
                } else // Odd = right slot
                {
                    equipmentSlot.GetButton().ExplicitNavigation(
                        Left: index > 0 && m_equipmentSlots[index - 1].GetButton().interactable ? m_equipmentSlots[index - 1].GetButton() : null,
                        Up: index > 1 && m_equipmentSlots[index - 2].GetButton().interactable ? m_equipmentSlots[index - 2].GetButton() : null,
                        Down: index < m_equipmentSlots.Length - 2 && m_equipmentSlots[index + 2].GetButton().interactable ? m_equipmentSlots[index + 2].GetButton() : null);
                }
            }
            m_equipmentSlots.FirstOrDefault(s => s.GetButton().interactable).GetButton().Select();
        }

        public void Close()
        {

        }

        public void Clean()
        {
            m_character = null;
        }
        #endregion
    }
}
