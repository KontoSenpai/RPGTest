using System;
using UnityEngine;
using RPGTest.Helpers;
using RPGTest.Inputs;
using RPGTest.UI.Widgets;
using RPGTest.Models.Items;
using RPGTest.Models.Entity;
using RPGTest.Enums;
using System.Linq;
using RPGTest.Models;
using static RPGTest.UI.Common.UI_PartyMember;

namespace RPGTest.UI.Common
{
    public class UI_EquipmentSlots : UI_Dialog
    {
        [SerializeField]
        private UI_EquipmentSlot[] m_equipmentSlots;

        [HideInInspector]
        public CancelActionHandler EquipActionCancelled { get; set; }
        [HideInInspector]
        public EquipmentSlotSelectedHandler EquipActionPerformed { get; set; }

        private PresetSlot m_presetSlot;
        private PlayableCharacter m_character;
        private Equipment m_selectedItem;

        public override void Awake()
        {
            base.Awake();

            m_playerInput = new Controls();
            m_playerInput.UI.Cancel.performed += OnCancel_performed;
        }

        #region Public Methods
        public void Initialize(PlayableCharacter character, Equipment item)
        {
            m_character = character;
            m_selectedItem = item;
            m_presetSlot = PresetSlot.First;

            var equipmentSlots = m_character.EquipmentSlots.GetEquipmentPreset(PresetSlot.First);
            foreach (UI_EquipmentSlot slot in m_equipmentSlots)
            {
                slot.Initialize(equipmentSlots[slot.Slot], m_selectedItem);
            }
        }

        public override void Open()
        {
            base.Open();

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

        public void Clean()
        {
            m_character = null;
        }

        public void OnSlotSelected(int slot)
        {
            EquipActionPerformed.Invoke(m_presetSlot, (Slot)slot);
        }
        #endregion


        #region Input Events
        private void OnCancel_performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            m_playerInput.Disable();
            EquipActionCancelled();
        }
        #endregion
    }
}
