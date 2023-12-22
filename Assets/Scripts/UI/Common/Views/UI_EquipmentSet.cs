using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPGTest.Helpers;
using RPGTest.Models.Items;
using RPGTest.Models.Entity;
using RPGTest.Enums;
using UnityEngine.UI;
using RPGTest.UI.Utils;
using RPGTest.Inputs;
using UnityEngine.InputSystem;
using RPGTest.Models;

namespace RPGTest.UI.Common
{
    public class UI_EquipmentSet : UI_View
    {
        public UI_PresetSlotSelector PresetSelector;

        [HideInInspector]
        public EquipmentSlot Slot { private set; get; }

        [HideInInspector]
        public PresetSlot Preset => PresetSelector.GetCurrentPreset();

        [HideInInspector]
        public CancelActionHandler EquipActionCancelled { get; set; }

        [HideInInspector]
        public GameObjectSelectionHandler SlotSelected { get; set; }

        [HideInInspector]
        public SlotSelectionHandler SlotConfirmed { get; set; }

        [HideInInspector]
        public ItemsSelectionHandler ItemUnequipped { get; set; }

        [SerializeField]
        private UI_InventoryItem[] m_equipmentComponents;

        [SerializeField]
        private bool EnableSecondaryAction;

        private PlayableCharacter m_character;

        private Equipment m_selectedItem;

        #region Public Methods
        public override void Awake()
        {
            base.Awake();

            m_playerInput.UI.Cancel.performed += OnCancel_performed;
            m_playerInput.UI.SecondaryNavigate.performed += OnSecondaryNavigate_performed;
            m_playerInput.UI.SecondaryAction.performed += ctx =>
            {
                OnSecondaryAction_performed(ctx);
            };

            foreach (var equipment in m_equipmentComponents)
            {
                equipment.ItemSelectionConfirmed += OnSlot_Selected;
            }
            PresetSelector.PresetSlotSelected += PresetSelector_PresetSlotSelected;
        }

        public override Dictionary<string, string[]> GetInputDisplay(Controls playerInput = null)
        {
            var controls = playerInput ?? m_playerInput;
            m_inputActions = new Dictionary<string, string[]>()
            {
                {
                    "Select Slot",
                    new string[]
                    {
                        "UI_" + controls.UI.Navigate.name
                    }
                },
                {
                    "Confirm",
                    new string[]
                    {
                        "UI_" + controls.UI.Submit.name,
                        "UI_" + controls.UI.LeftClick.name,
                    }
                },
                {
                    "Remove Equipment",
                    new string[]
                    {
                        "UI_" + controls.UI.SecondaryAction.name,
                    }
                },
                {
                    "Change Preset",
                    new string[]
                    {
                        "UI_" + controls.UI.SecondaryNavigate.name,
                    }
                }
            };

            return m_inputActions;
        }

        protected override void UpdateInputActions()
        {
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

        public void Select(EquipmentSlot pendingSlot = EquipmentSlot.None)
        {
            base.Select();

            InitializeSlotsState();

            EventSystemEvents.OnSelectionUpdated += OnSelection_Updated;

            if (pendingSlot != EquipmentSlot.None)
            {
                m_equipmentComponents.Single((c) => c.Slot == pendingSlot).GetComponent<Button>().Select();
            }
            else
            {
                m_equipmentComponents.FirstOrDefault(c => c.GetComponent<Button>().interactable).GetComponent<Button>().Select();
            }
        }

        public override void Deselect()
        {
            base.Deselect();
            m_equipmentComponents.ForEach((c) => c.GetComponent<Button>().interactable = false);

            EventSystemEvents.OnSelectionUpdated -= OnSelection_Updated;
        }

        public void Clean()
        {
            m_character = null;
        }

        public void ChangePreset()
        {
            PresetSelector.ChangePreset();
        }

        public Equipment GetEquipment()
        {
            var equipmentSlots = m_character.EquipmentComponent.GetEquipmentSlots(PresetSelector.GetCurrentPreset());

            return equipmentSlots[Slot];
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
                if (Slot != component.Slot)
                {
                    Slot = component.Slot;
                }
                SlotSelected(component.gameObject);
            }
        }
        #endregion

        #region Input Events
        /// <summary>
        /// Secondary action = unequip item
        /// </summary>
        /// <param name="ctx"></param>
        protected void OnSecondaryAction_performed(InputAction.CallbackContext ctx)
        {
            if (!EnableSecondaryAction)
            {
                return;
            }

            m_character.EquipmentComponent.TryUnequip(PresetSelector.GetCurrentPreset(), Slot, out var removedEquipments);

            if (!removedEquipments.Any())
            {
                return;
            }

            Refresh();
            ItemUnequipped(removedEquipments);
        }

        private void OnCancel_performed(InputAction.CallbackContext ctx)
        {
            DisableControls();
            if (m_selectedItem != null)
            {
                EquipActionCancelled();
            }
        }

        private void OnSecondaryNavigate_performed(InputAction.CallbackContext ctx)
        {
            if (ctx.ReadValue<Vector2>().magnitude > 0.4f)
            {
                ChangePreset();
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
            var equipmentSlots = m_character.EquipmentComponent.GetEquipmentSlots();
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
                m_equipmentComponents.ForEach(s => s.GetComponent<UI_EquipmentPreview>().Enable(s.Slot < EquipmentSlot.Head));
            }
            else if (m_selectedItem.EquipmentType < EquipmentType.HeavyArmor) // Heads
            {
                m_equipmentComponents.ForEach(s => s.GetComponent<UI_EquipmentPreview>().Enable(s.Slot < EquipmentSlot.Body));
            }
            else if (m_selectedItem.EquipmentType < EquipmentType.Accessory) // Armor
            {
                m_equipmentComponents.ForEach(s => s.GetComponent<UI_EquipmentPreview>().Enable(s.Slot < EquipmentSlot.Accessory1));
            }
            else // Accessories
            {
                m_equipmentComponents.ForEach(s => s.GetComponent<UI_EquipmentPreview>().Enable(s.Slot > EquipmentSlot.Body));
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
