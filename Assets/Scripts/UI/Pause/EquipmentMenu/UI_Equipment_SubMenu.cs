using RPGTest.Managers;
using System.Collections.Generic;
using UnityEngine;
using RPGTest.Models.Entity;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using RPGTest.Models;
using RPGTest.UI.Common;
using RPGTest.UI.PartyMenu;
using RPGTest.UI.Utils;
using RPGTest.UI.Widgets;

namespace RPGTest.UI
{
    public class UI_Equipment_SubMenu : UI_Pause_SubMenu
    {
        [SerializeField] private UI_Equipment_View EquipmentView;

        [SerializeField] private UI_EquipmentList ItemList;

        [SerializeField] private UI_PartyMember CharacterInfo;

        [SerializeField] private UI_Party_Member_Stats CharacterStats;

        private PlayableCharacter m_selectedCharacter;
        private PresetSlot m_currentPreset;

        //Items control
        private int m_currentNavigationIndex = 0;
        private bool m_actionInProgress = false;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;
        private InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;

        private List<PlayableCharacter> m_characters => m_partyManager.GetAllPartyMembers();
        // Categories to display in the ItemList
        private List<ItemFilterCategory> m_filterCategories = new List<ItemFilterCategory>()
        {
            ItemFilterCategory.Weapons,
            ItemFilterCategory.Armors,
            ItemFilterCategory.Accessories,
        };

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
                //if (m_ActionMenuOpened) return;
                //m_performTimeStamp = Time.time + 0.3f;
                //Navigate_Performed(ctx.ReadValue<Vector2>());
            };
            m_playerInput.UI.SecondaryNavigate.performed += ctx =>
            {
                SecondaryNavigate_Performed(ctx.ReadValue<Vector2>());
            };
            m_playerInput.UI.Navigate.canceled += ctx => {
                m_navigateStarted = false;
            };
            m_playerInput.UI.Submit.performed += ctx =>
            {
                //Submit_Performed();
            };
            m_playerInput.UI.SecondaryAction.performed += ctx =>
            {
                //SecondaryAction_Performed();
            };
            m_playerInput.UI.MouseMoved.performed += ctx =>
            {
                //MouseMoved_Performed();
            };
            m_playerInput.UI.RightClick.performed += ctx =>
            {
                //MouseRightClick_Performed();
            };
        }

        public void Update()
        {
            if (m_navigateStarted && (Time.time - m_performTimeStamp) >= WaitTimeBetweenPerforms)
            {
                m_performTimeStamp = Time.time;
                //Navigate_Performed(m_playerInput.UI.Navigate.ReadValue<Vector2>());
            }
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

        public override void Open(Dictionary<string, object> parameters)
        {
            base.Open(parameters);

            if (parameters.TryGetValue("CharacterIndex", out var value))
            {
                m_currentNavigationIndex = (int)value;
            }
            else
            {
                m_currentNavigationIndex = 0;
            }
            Initialize();

            UpdateInputActions();
            EventSystemEvents.OnSelectionUpdated += OnSelection_Updated;
        }

        public override void Initialize(bool refreshAll = true)
        {
            if (ItemList)
            {
                ItemList.Initialize(GetItemsToDisplay(), m_filterCategories);
            }
        }

        public override void CloseMenu()
        {
            EventSystemEvents.OnSelectionUpdated += OnSelection_Updated;
            base.CloseMenu();
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
        public void OnFirstPresetSelected()
        {
            if (m_currentPreset != PresetSlot.First)
                OnPresetChangedInternal(PresetSlot.First);
        }

        public void OnSecondPresetSelected()
        {
            if (m_currentPreset != PresetSlot.Second)
                OnPresetChangedInternal(PresetSlot.Second);
        }

        private void OnSelection_Updated(GameObject currentSelection, GameObject previousSelection)
        {
            return;
            if (currentSelection != null && currentSelection.TryGetComponent<UI_InventoryItem>(out var component))
            {
                switch (component.Slot)
                {
                    case Enums.Slot.LeftHand:
                    case Enums.Slot.RightHand:
                        ItemList.Filter(ItemFilterCategory.Weapons);
                        break;
                    case Enums.Slot.Head:
                    case Enums.Slot.Body:
                        ItemList.Filter(ItemFilterCategory.Armors);
                        break;
                    case Enums.Slot.Accessory1:
                    case Enums.Slot.Accessory2:
                        ItemList.Filter(ItemFilterCategory.Accessories);
                        break;
                    default:
                        Debug.LogError($"Unrecognized Slot type {component.Slot}");
                        break;
                }
                RefreshItemList();
            }
        }
        #endregion

        #region Input Events
        private void MouseMoved_Performed()
        {
            FindObjectOfType<EventSystem>().SetSelectedGameObject(null);
            m_currentNavigationIndex = -1;
        }

        private void CycleCharacters_Performed(InputAction.CallbackContext ctx)
        {
            var movement = ctx.ReadValue<float>();
            if (movement > 0)
            {
                m_currentNavigationIndex++;
            } else
            {
                m_currentNavigationIndex--;
            }
            RefreshInternal();
        }

        private void SecondaryNavigate_Performed(Vector2 movement)
        {
            if ((movement.x < -0.4f || movement.x > 0.04f))
            {
                m_currentPreset = m_currentPreset == PresetSlot.First ? PresetSlot.Second : PresetSlot.First;
                RefreshInternal();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Update current slot selected, and refresh character information to reflect the newly selected slot information
        /// </summary>
        /// <param name="slot">Selected Slot</param>
        private void OnPresetChangedInternal(PresetSlot slot)
        {
            m_currentPreset = slot;
            RefreshInternal();
        }

        private void Initialize()
        {
            EquipmentView.InitializeSelection();
        }

        /// <summary>
        /// 
        /// </summary>
        private void RefreshInternal()
        {
            var selectedCharacter = m_characters[m_currentNavigationIndex];
            
            if (EquipmentView)
            {
                EquipmentView.Refresh(selectedCharacter.EquipmentSlots, m_currentPreset);
            }

            if (CharacterInfo)
            {
                CharacterInfo.Refresh(selectedCharacter, m_currentPreset);
            }

            if (CharacterStats)
            {
                CharacterStats.Refresh(selectedCharacter, m_currentPreset);
            }
        }

        private void RefreshItemList()
        {
            if (ItemList)
            {
                // TODO : re-do the refresh
                //ItemList.UpdateItems(GetItemsToDisplay());
            }
        }

        private List<UIItemDisplay> GetItemsToDisplay()
        {
            var displayItems = new List<UIItemDisplay>();

            foreach (var item in m_inventoryManager.GetItemsOfType(Enums.ItemType.Equipment))
            {
                displayItems.Add(
                    new UIItemDisplay
                    {
                        Item = item.Key,
                        Quantity = item.Value,
                    });
            };
            return displayItems;
        }
        #endregion
    }
}
