using System;
using UnityEngine;
using RPGTest.Helpers;
using RPGTest.Models.Items;
using RPGTest.Models.Entity;
using RPGTest.Enums;
using System.Linq;
using static RPGTest.UI.Common.UI_View_EntityInfos;
using UnityEngine.UI;
using System.Collections.Generic;
using RPGTest.UI.Utils;

namespace RPGTest.UI.Common
{
    public class UI_EquipmentSet : UI_View
    {
        [SerializeField] private UI_PresetSlotSelector PresetSelector;

        [SerializeField]
        private UI_InventoryItem[] m_equipmentComponents;

        [HideInInspector]
        public CancelActionHandler EquipActionCancelled { get; set; }
        
        [HideInInspector]
        public EquipmentSlotSelectedHandler SlotSelected { get; set; }

        [HideInInspector]
        public EquipmentSlotSelectedHandler SlotConfirmed { get; set; }

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
                equipment.ItemSelectionConfirmed += OnSlot_Selected;
            }
            PresetSelector.PresetSlotSelected += PresetSelector_PresetSlotSelected;
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
        /// Initialize content of the Panel to reflect information of given character
        /// </summary>
        /// <param name="character">Character we need to refresh the display for</param>
        public void Initialize(PlayableCharacter character)
        {
            m_character = character;
            PresetSelector.Initialize();

            m_playerInput.UI.SecondaryAction.performed -= OnSecondaryAction_performed;
            m_playerInput.UI.Cancel.performed -= OnCancel_performed;
        }

        /// <summary>
        /// Initialize content of the Panel to reflect information of given character and previewItem
        /// </summary>
        /// <param name="character">Character we need to refresh the display for</param>
        /// <param name="item"></param>
        public void Initialize(PlayableCharacter character, Equipment item)
        {
            m_character = character;
            m_selectedItem = item;

            PresetSelector.Initialize();
        }

        /// <summary>
        /// Refresh slots for currently selected character
        /// </summary>
        public void Refresh()
        {
            RefreshEquipmentComponents();
        }

        public void Select(Slot pendingSlot = Slot.None)
        {
            EnableControls();
            UpdateInputActions();

            InitializeSlotsState();

            EventSystemEvents.OnSelectionUpdated += OnSelection_Updated;

            if (pendingSlot != Slot.None)
            {
                m_equipmentComponents.Single((c) => c.Slot == pendingSlot).GetComponent<Button>().Select();
            }
            else
            {
                m_equipmentComponents.FirstOrDefault(c => c.GetComponent<Button>().interactable).GetComponent<Button>().Select();
            }
        }

        public void Deselect()
        {
            m_equipmentComponents.ForEach((c) => c.GetComponent<Button>().interactable = false);

            EventSystemEvents.OnSelectionUpdated -= OnSelection_Updated;

            DisableControls();
        }

        public void Clean()
        {
            m_character = null;
        }

        public void ChangePreset()
        {
            PresetSelector.ChangePreset();
        }
        #endregion

        #region Events
        private void PresetSelector_PresetSlotSelected(object sender, PresetSlotSelectedEventArgs e)
        {
            OnPresetChanged();
        }

        public void OnSlot_Selected(object sender, ItemSelectionConfirmedEventArgs args)
        {
            m_equipmentComponents.Single((c) => c.Slot == args.Slot).GetComponent<Button>().interactable = false;
            SlotConfirmed.Invoke(PresetSelector.GetCurrentPreset(), args.Slot);
        }
        #endregion

        #region Input Events
        public void OnSecondaryAction_performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            ChangePreset();
        }

        private void OnCancel_performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            DisableControls();
            if (m_selectedItem != null)
            {
                EquipActionCancelled();
            }
        }

        /// <summary>
        /// Handled from the UIEventSystem, whenever a different <see cref="Selectable"/> changes
        /// </summary>
        /// <param name="currentSelection"></param>
        /// <param name="previousSelection"></param>
        public void OnSelection_Updated(GameObject currentSelection, GameObject previousSelection)
        {
            if (currentSelection != null && m_equipmentComponents.Any((i) => i.gameObject == currentSelection))
            {
                var component = m_equipmentComponents.Single((c) => c.gameObject == currentSelection);
                SlotSelected(PresetSelector.GetCurrentPreset(), component.Slot);
            }
        }
        #endregion

        #region Private Methods
        private void OnPresetChanged()
        {
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

        private void RefreshEquipmentComponents()
        {
            var equipmentSlots = m_character.EquipmentSlots.GetEquipmentPreset(PresetSelector.GetCurrentPreset());
            foreach (var component in m_equipmentComponents)
            {
                component.Initialize(equipmentSlots[component.Slot], -1);
                component.GetComponent<UI_EquipmentPreview>().Initialize(m_selectedItem);
            }
        }

        private void InitializeSlotsState()
        {
            if (m_selectedItem == null)
            {
                m_equipmentComponents.ForEach((c) => c.GetComponent<Button>().interactable = true);
                return;
            }

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
