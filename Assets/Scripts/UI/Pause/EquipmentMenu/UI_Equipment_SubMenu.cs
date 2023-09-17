﻿using RPGTest.Managers;
using System.Collections.Generic;
using UnityEngine;
using RPGTest.Models.Entity;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using RPGTest.Models;
using RPGTest.UI.Common;
using RPGTest.Enums;
using RPGTest.Models.Items;
using System.Linq;

namespace RPGTest.UI
{
    public enum ActionStage
    {
        SelectSlot,
        SelectItem,

    }
    public class UI_Equipment_SubMenu : UI_Pause_SubMenu
    {
        [SerializeField] private UI_CharacterFilters CharacterFilters;

        [SerializeField] private UI_EquipmentSet EquipmentSet;

        [SerializeField] private UI_ItemList ItemList;
        [SerializeField] private UI_ItemListFilters ItemListFilters;
        [SerializeField] private UI_ItemListUpdator ItemListUpdator;

        [SerializeField] private UI_View_EntityContainer EntityComponentsContainer;

        //Items control
        private int m_currentNavigationIndex = 0;
        private bool m_actionInProgress = false;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;
        private InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;

        private ActionStage m_currentActionStage = ActionStage.SelectSlot;

        private List<PlayableCharacter> m_characters => m_partyManager.GetAllPartyMembers();
        // Categories to display in the ItemList
        private List<ItemFilterCategory> m_filterCategories = new List<ItemFilterCategory>()
        {
            ItemFilterCategory.Weapons,
            ItemFilterCategory.Armors,
            ItemFilterCategory.Accessories,
        };

        private PendingItemSelection m_pendingItemSelection;

        public override void Awake()
        {
            base.Awake();
            m_playerInput.UI.Navigate.started += ctx => {
                m_navigateStarted = true;
            };
            m_playerInput.UI.Cycle.started += ctx =>
            {
                CycleCharacters_Performed(ctx);
            };
            m_playerInput.UI.Navigate.performed += ctx =>
            {
                // if (m_ActionMenuOpened) return;
                // m_performTimeStamp = Time.time + 0.3f;
                // Navigate_Performed(ctx.ReadValue<Vector2>());
            };
            m_playerInput.UI.SecondaryNavigate.performed += ctx =>
            {
                SecondaryNavigate_Performed(ctx.ReadValue<Vector2>());
            };
            m_playerInput.UI.Navigate.canceled += ctx => {
                m_navigateStarted = false;
            };
            m_playerInput.UI.Cancel.performed += ctx =>
            {
                OnCancel_performed(ctx);
            };
            m_playerInput.UI.Submit.performed += ctx =>
            {
                // Submit_Performed();
            };
            m_playerInput.UI.SecondaryAction.performed += ctx =>
            {
                OnSecondaryAction_Performed(ctx);
            };
            m_playerInput.UI.MouseMoved.performed += ctx =>
            {
                //MouseMoved_Performed();
            };
            m_playerInput.UI.RightClick.performed += ctx =>
            {
                //MouseRightClick_Performed();
            };

            CharacterFilters.CharacterFilterSelected += OnCharacter_Selected;

            // Todo: Preview
            EquipmentSet.SlotSelected += OnEquipmentSlot_Selected;
            EquipmentSet.SlotConfirmed += OnEquipmentSlot_Confirmed;

            ItemList.ItemSelectionChanged += OnItemSelection_Changed;
            ItemListFilters.FilterChanged += OnFilter_Changed;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            UpdateInputActions();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        public override void Initialize()
        {
            CharacterFilters.Initialize();
        }

        public override void OpenSubMenu(Dictionary<string, object> parameters)
        {
            base.OpenSubMenu(parameters);

            var characterIndex = -1;
            if (parameters.TryGetValue("CharacterIndex", out var value) && m_partyManager.GetActivePartyMembers()[(int)value] != null)
            {
                characterIndex = (int)value;
            }

            var items = ItemListUpdator.InstantiateItems(m_inventoryManager.GetItems());
            items.ForEach((i) =>
            {
                i.GetComponent<UI_InventoryItem>().ItemSelectionConfirmed += OnItemSelection_Confirmed;
            });

            ItemList.Initialize(items);
            ItemListFilters.Initialize();

            CharacterFilters.Open(characterIndex);

            UpdateInputActions();
        }

        public override void CloseSubMenu()
        {
            ItemList.Clear();
            CharacterFilters.Close();
            EquipmentSet.Deselect();

            base.CloseSubMenu();
        }

        public override void ExitPause()
        {
            ItemList.Clear();
            EquipmentSet.Deselect();

            base.ExitPause();
        }

        protected override void UpdateInputActions()
        {
            m_inputActions = new Dictionary<string, string[]>()
            {
                {
                    "Navigate",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Navigate.name
                    }
                },
                {
                    "Change Character",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cycle.name
                    }
                },
                {
                    "Select",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Submit.name,
                        "UI_" + m_playerInput.UI.LeftClick.name
                    }
                }
            };

            if (m_actionInProgress)
            {
                m_inputActions.Add("Cancel",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cancel.name
                    });
            }
            else
            {
                m_inputActions.Add("Exit Menu",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cancel.name
                    });
            }
            base.UpdateInputActions();
        }

        #region Event
        private void OnEquipmentSlot_Selected(PresetSlot preset, Slot slot)
        {
            switch(slot)
            {
                case Slot.LeftHand:
                case Slot.RightHand:
                    ItemListFilters.Filter(ItemFilterCategory.Weapons);
                    break;
                case Slot.Head:
                    ItemListFilters.Filter(ItemFilterCategory.Head);
                    break;
                case Slot.Body:
                    ItemListFilters.Filter(ItemFilterCategory.Body);
                    break;
                case Slot.Accessory1:
                case Slot.Accessory2:
                    ItemListFilters.Filter(ItemFilterCategory.Accessories);
                    break;
                default:
                    throw new System.Exception($"Unhandled slot type {slot}");
            }

            m_pendingItemSelection = new PendingItemSelection()
            {
                Preset = preset,
                Slot = slot,
            };
        }

        private void OnEquipmentSlot_Confirmed(PresetSlot preset, Slot slot)
        {
            m_pendingItemSelection = new PendingItemSelection()
            {
                Preset = preset,
                Slot = slot,
            };

            ChangeStage(ActionStage.SelectItem);
        }

        private void OnSelection_Updated(GameObject currentSelection, GameObject previousSelection)
        {
            if (currentSelection != null && currentSelection.TryGetComponent<UI_InventoryItem>(out var component))
            {
                switch (component.Slot)
                {
                    case Enums.Slot.LeftHand:
                    case Enums.Slot.RightHand:
                        ItemList.ChangeItemsVisibility(ItemFilterCategory.Weapons);
                        break;
                    case Enums.Slot.Head:
                    case Enums.Slot.Body:
                        ItemList.ChangeItemsVisibility(ItemFilterCategory.Armors);
                        break;
                    case Enums.Slot.Accessory1:
                    case Enums.Slot.Accessory2:
                        ItemList.ChangeItemsVisibility(ItemFilterCategory.Accessories);
                        break;
                    default:
                        Debug.LogError($"Unrecognized Slot type {component.Slot}");
                        break;
                }
            }
        }

        /// <summary>
        /// Handle item selection changes from the connect <see cref="ItemList"/>
        /// </summary>
        /// <param name="itemComponent"></param>
        public void OnItemSelection_Changed(GameObject itemComponent)
        {
            if (itemComponent != null && itemComponent.TryGetComponent<UI_InventoryItem>(out var component))
            {
                EntityComponentsContainer.Preview(m_pendingItemSelection.Preset, m_pendingItemSelection.Slot, (Equipment)component.Item);
            }
        }

        /// <summary>
        /// Handle item selection confirmation event from the <see cref="ItemList"/>
        /// If the pending item selection have an owner, unequip from previous owner
        /// </summary>
        /// <param name="args"></param>
        public void OnItemSelection_Confirmed(object sender, ItemSelectionConfirmedEventArgs e)
        {
            if (e.Owner != null)
            {
                e.Owner.TryUnequip(m_pendingItemSelection.Preset, e.Slot, out var _);
            }

            var character = CharacterFilters.GetCurrentCharacter();
            character.TryEquip(m_pendingItemSelection.Preset, m_pendingItemSelection.Slot, (Equipment)e.Item, out var removedEquipments);

            EquipmentSet.Refresh();

            var itemUpdates = new List<ItemUpdate>();
            itemUpdates.Add(new ItemUpdate(e.Item, m_inventoryManager.GetHeldItemQuantity(e.Item.Id)));
            foreach (var removedItem in removedEquipments)
            {
                itemUpdates.Add(new ItemUpdate(removedItem, m_inventoryManager.GetHeldItemQuantity(removedItem.Id)));
            }

            var items = ItemListUpdator.UpdateItems(ItemList.GetItems().Select((i) => i.gameObject).ToList(), itemUpdates);
            ItemList.UpdateItems(items);

            EntityComponentsContainer.Refresh(m_pendingItemSelection.Preset);

            ChangeStage(ActionStage.SelectSlot);
        }

        /// <summary>
        /// Handle filter changes fired from <see cref="ItemListFilters"/>
        /// </summary>
        /// <param name="filter">New filter</param>
        public void OnFilter_Changed(ItemFilterCategory filter)
        {
            ItemList.ChangeItemsVisibility(filter);
        }
        
        private void OnCharacter_Selected(object sender, CharacterFilterSelectedEventArgs e)
        {
            ChangeCharacter(e.Character);
        }
        #endregion

        #region Input Events
        protected override void OnCancel_performed(InputAction.CallbackContext ctx)
        {
            switch (m_currentActionStage)
            {
                case ActionStage.SelectSlot:
                    ExitPause();
                    break;
                case ActionStage.SelectItem:
                    ChangeStage(ActionStage.SelectSlot);
                    break;
            }
        }

        /// <summary>
        /// Sec
        /// </summary>
        /// <param name="ctx"></param>
        protected void OnSecondaryAction_Performed(InputAction.CallbackContext ctx)
        {
            if (m_currentActionStage != ActionStage.SelectSlot)
            {
                return;
            }

            var character = CharacterFilters.GetCurrentCharacter();

            character.TryUnequip(m_pendingItemSelection.Preset, m_pendingItemSelection.Slot, out var removedEquipments);

            if (!removedEquipments.Any())
            {
                return;
            }

            var itemUpdates = new List<ItemUpdate>();
            foreach (var removedItem in removedEquipments)
            {
                itemUpdates.Add(new ItemUpdate(removedItem, m_inventoryManager.GetHeldItemQuantity(removedItem.Id)));
            }

            EquipmentSet.Refresh();
            var items = ItemListUpdator.UpdateItems(ItemList.GetItems().Select((i) => i.gameObject).ToList(), itemUpdates);
            ItemList.UpdateItems(items);

            EntityComponentsContainer.Refresh(m_pendingItemSelection.Preset);
        }

        private void MouseMoved_Performed()
        {
            FindObjectOfType<EventSystem>().SetSelectedGameObject(null);
            m_currentNavigationIndex = -1;
        }

        private void CycleCharacters_Performed(InputAction.CallbackContext ctx)
        {
            var movement = ctx.ReadValue<float>();
            CharacterFilters.ChangeCharacter(movement > 0);
        }

        private void SecondaryNavigate_Performed(Vector2 movement)
        {
            if (m_currentActionStage == ActionStage.SelectSlot && Mathf.Abs(movement.x) > 0.4f)
            {
                EquipmentSet.ChangePreset();
            }
        }
        #endregion

        #region Private Methods
        private void ChangeStage(ActionStage stage)
        {
            m_currentActionStage = stage;

            if (stage == ActionStage.SelectSlot)
            {
                FindObjectOfType<EventSystem>().SetSelectedGameObject(null);
                ItemList.ChangeItemsSelectability(false);
                EquipmentSet.Select(m_pendingItemSelection?.Slot ?? Slot.None);
                EntityComponentsContainer.Unpreview();
            }
            else if (stage == ActionStage.SelectItem)
            {
                EquipmentSet.Deselect();

                ItemList.ChangeItemsSelectability(true);
                ItemList.SelectDefault();
            }
        }

        private void ChangeCharacter(PlayableCharacter character)
        {
            EntityComponentsContainer.Initialize(character, PresetSlot.First);
            EquipmentSet.Initialize(character);

            ChangeStage(ActionStage.SelectSlot);
        }
        #endregion
    }
}
