using System;
using UnityEngine;
using RPGTest.Helpers;
using RPGTest.Models.Items;
using RPGTest.Models.Entity;
using RPGTest.Enums;
using System.Linq;
using RPGTest.Models;
using static RPGTest.UI.Common.UI_PartyMember;
using UnityEngine.UI;
using System.Collections.Generic;

namespace RPGTest.UI.Common
{
    public class UI_EquipmentSlots : UI_Dialog
    {
        [SerializeField]
        private Button FirstPreset;
        [SerializeField]
        private Button SecondPreset;
        [SerializeField]
        private UI_InventoryItem[] m_equipmentComponents;
        [HideInInspector]
        public CancelActionHandler EquipActionCancelled { get; set; }
        [HideInInspector]
        public EquipmentSlotSelectedHandler EquipActionPerformed { get; set; }

        private PresetSlot m_presetSlot;
        private PlayableCharacter m_character;
        private Equipment m_selectedItem;

        #region Public Methods
        public override void Awake()
        {
            base.Awake();

            m_playerInput.UI.SecondaryAction.performed += OnSecondaryAction_performed;

            m_playerInput.UI.Cancel.performed += OnCancel_performed;

            foreach (var equipment in m_equipmentComponents)
            {
                equipment.GetComponent<Button>().onClick.AddListener(() => OnSlotSelected(equipment.Slot));
            }
            FirstPreset.onClick.AddListener(() =>
            {
                m_presetSlot = PresetSlot.First;
                OnPresetChanged();
            });
            SecondPreset.onClick.AddListener(() =>
            {
                m_presetSlot = PresetSlot.Second;
                OnPresetChanged();
            });
        }

        public override void Open()
        {
            gameObject.SetActive(true);
            m_playerInput.Disable();
        }

        public override void Close()
        {
            gameObject.SetActive(false);
            m_playerInput.Disable();
        }

        protected override void UpdateInputActions()
        {
            m_inputActions = new Dictionary<string, string[]>()
            {
                {
                    "Select Slot",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Navigate.name
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
                    "Cancel",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cancel.name,
                        "UI_" + m_playerInput.UI.RightClick.name,
                    }
                },
                {
                    "Change Preset",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.SecondaryAction.name,
                    }
                }
            };
            base.UpdateInputActions();
        }


        /// <summary>
        /// Refresh content of the Panel to reflect information of given character
        /// </summary>
        /// <param name="character"></param>
        /// <param name="item"></param>
        public void Refresh(PlayableCharacter character, Equipment item)
        {
            m_character = character;
            m_selectedItem = item;
            m_presetSlot = PresetSlot.First;

            OnPresetChanged();
        }

        public void Select()
        {
            m_playerInput.Enable();

            m_equipmentComponents.FirstOrDefault(s => s.GetComponent<Button>().interactable).GetComponent<Button>().Select();
        }

        public void Clean()
        {
            m_character = null;
        }

        public void ChangePreset()
        {
            m_presetSlot = m_presetSlot == PresetSlot.First ? PresetSlot.Second : PresetSlot.First;
            OnPresetChanged();
        }
        #endregion

        #region Events

        public void OnPresetChanged()
        {
            FirstPreset.interactable = m_presetSlot == PresetSlot.Second;
            SecondPreset.interactable = m_presetSlot == PresetSlot.First;

            RefreshEquipmentComponents();
            InitializeSlotsState();
            ExplicitNavigation();

            if (m_playerInput.asset.enabled)
            {
                var equipmentSlot = m_equipmentComponents.FirstOrDefault(s => s.GetComponent<Button>().interactable);
                equipmentSlot.GetComponent<Button>().Select();
                equipmentSlot.GetComponent<UI_EquipmentPreview>().PreviewItem(true);
            }
        }

        public void OnSlotSelected(Slot slot)
        {
            EquipActionPerformed.Invoke(m_presetSlot, slot);
        }
        #endregion


        #region Input Events
        public void OnSecondaryAction_performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            ChangePreset();
        }

        private void OnCancel_performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            m_playerInput.Disable();
            EquipActionCancelled();
        }
        #endregion

        #region Private Methods
        private void RefreshEquipmentComponents()
        {
            var equipmentSlots = m_character.EquipmentSlots.GetEquipmentPreset(m_presetSlot);
            foreach (var component in m_equipmentComponents)
            {
                component.Initialize(equipmentSlots[component.Slot], -1);
                component.GetComponent<UI_EquipmentPreview>().Initialize(m_selectedItem);
            }
        }

        private void InitializeSlotsState()
        {
            if (m_selectedItem.EquipmentType < EquipmentType.Helmet) // Weapons
            {
                m_equipmentComponents.ForEach(s => s.GetComponent<UI_EquipmentPreview>().Enable(s.Slot < Slot.Head));
            }
            else if (m_selectedItem.EquipmentType < EquipmentType.HeavyArmor) // Heads
            {
                m_equipmentComponents.ForEach(s => s.GetComponent<UI_EquipmentPreview>().Enable(s.Slot < Slot.Body));
            }
            else if (m_selectedItem.EquipmentType < EquipmentType.Accessory) // Armor
            {
                m_equipmentComponents.ForEach(s => s.GetComponent<UI_EquipmentPreview>().Enable(s.Slot < Slot.Accessory1));
            }
            else // Accessories
            {
                m_equipmentComponents.ForEach(s => s.GetComponent<UI_EquipmentPreview>().Enable(s.Slot > Slot.Body));
            }
        }

        private void ExplicitNavigation()
        {
            foreach (var component in m_equipmentComponents)
            {
                var index = Array.IndexOf(m_equipmentComponents, component);
                //slot.SlotSelected
                if (index % 2 == 0) // Even = left slot
                {
                    component.GetComponent<Button>().ExplicitNavigation(
                        Right: index < m_equipmentComponents.Length - 1 && m_equipmentComponents[index + 1].GetComponent<Button>().interactable ? m_equipmentComponents[index + 1].GetComponent<Button>() : null,
                        Up: index > 1 && m_equipmentComponents[index - 2].GetComponent<Button>().interactable ? m_equipmentComponents[index - 2].GetComponent<Button>() : null,
                        Down: index < m_equipmentComponents.Length - 2 && m_equipmentComponents[index + 2].GetComponent<Button>().interactable ? m_equipmentComponents[index + 2].GetComponent<Button>() : null);
                }
                else // Odd = right slot
                {
                    component.GetComponent<Button>().ExplicitNavigation(
                        Left: index > 0 && m_equipmentComponents[index - 1].GetComponent<Button>().interactable ? m_equipmentComponents[index - 1].GetComponent<Button>() : null,
                        Up: index > 1 && m_equipmentComponents[index - 2].GetComponent<Button>().interactable ? m_equipmentComponents[index - 2].GetComponent<Button>() : null,
                        Down: index < m_equipmentComponents.Length - 2 && m_equipmentComponents[index + 2].GetComponent<Button>().interactable ? m_equipmentComponents[index + 2].GetComponent<Button>() : null);
                }
            }
        }
        #endregion
    }
}
