using RPGTest.Enums;
using RPGTest.Managers;
using RPGTest.UI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPGTest.UI.PartyMenu
{
    public class UI_Party_SubMenu : UI_Pause_SubMenu
    {
        [SerializeField] private UI_PartyList PartyList;

        [SerializeField] private UI_View_EntityContainer EntityComponentsContainer;

        [SerializeField] private GameObject SubActionMenu;

        [SerializeField] private UI_PresetSlotSelector PresetSlotSelector;

        [SerializeField] private TextMeshProUGUI Money;
        [SerializeField] private TextMeshProUGUI Location;

        private UI_View_EntityContainer EntityComponentContainer => FindObjectOfType<UI_View_EntityContainer>();

        // Swap controls
        private bool m_swapInProgress = false;

        // SubMenu controls
        private bool m_ActionMenuOpened = false;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        private int m_inventoryMenuIndex = 1; // todo : need to keep that const elsewhere
        private int m_skillsMenuIndex = 2; // todo: same as line above

        private GameObject m_currentCharacter;
        private GameObject m_selectedCharacter;

        public override void Awake()
        {
            base.Awake();

            PartyList.SecondaryActionPerformed += OnSecondaryAction_performed;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            PartyList.CharacterSelectionChanged += OnCharacterSelection_changed;
            PartyList.CharacterSelectionConfirmed += OnCharacterSelection_confirmed;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            PartyList.CharacterSelectionChanged -= OnCharacterSelection_changed;
            PartyList.CharacterSelectionConfirmed -= OnCharacterSelection_confirmed;
        }

        public override void Initialize()
        {
            Money.text = FindObjectOfType<InventoryManager>().Money.ToString();
            Location.text = SceneManager.GetActiveScene().name;

            PartyList.Initialize(m_partyManager.GetActivePartyMembers(), m_partyManager.GetInactivePartyMembers(), m_partyManager.GetGuestCharacter());
        }

        public override void OpenSubMenu(Dictionary<string, object> parameters)
        {
            base.OpenSubMenu(parameters);
            m_playerInput.UI.Cancel.performed += OnCancel_performed;

            SubActionMenu.SetActive(false);
            PartyList.Select();
            UpdateInputActions();
        }

        public override void CloseSubMenu()
        {
            base.CloseSubMenu();
        }

        public void Start()
        {
        }

        #region Input Events
        // Cancel current action (swap or sub menu)
        protected override void OnCancel_performed(InputAction.CallbackContext obj)
        {
            CancelCurrentAction();
        }

        // Cycle Presets
        private void SecondaryNavigate_Performed(Vector2 movement)
        {
            if (m_currentCharacter != null && EntityComponentContainer != null && (movement.x < -0.4f || movement.x > 0.04f))
            {
                // TODO preset refresh
                PresetSlotSelector.ChangePreset();
                EntityComponentContainer.Refresh(PresetSlotSelector.GetCurrentPreset());
            }
        }

        // Deselect Button
        private void MouseMoved_Performed()
        {
            FindObjectOfType<EventSystem>().SetSelectedGameObject(null);
        }

        public void MouseRightClick_Performed()
        {
            if (m_swapInProgress)
            {
                CancelSwapInProgress();
                return;
            }

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                pointerId = -1,
            };

            pointerData.position = new Vector2(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue());

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            if (results.All(r => r.gameObject.GetComponent<UI_View_EntityInfos>() == null)) {
                SubActionMenu.SetActive(false);
            } else
            {
                var result = results.SingleOrDefault(r => r.gameObject.GetComponent<UI_View_EntityInfos>());
                OpenCharacterActionsMenu(result.gameObject);
            }
        }

        private void Debug_performed(InputAction.CallbackContext obj)
        {
        }
        #endregion

        #region Event Handlers 
        /// <summary>
        /// Handle event raised from the PartyList component.
        /// Opens a menu to select actions
        /// </summary>
        private void OnSecondaryAction_performed(object sender, EventArgs e)
        {
            OpenCharacterActionsMenu(m_currentCharacter);
        }

        /// <summary>
        /// Handle events raised from the PartyList character selection change
        /// </summary>
        /// <param name="characterGo"></param>
        private void OnCharacterSelection_changed(GameObject selectedCharacter)
        {
            if (selectedCharacter != null && selectedCharacter.TryGetComponent<UI_View_EntityInfos>(out var component))
            {
                if (component.GetPlayableCharacter() == null)
                {
                    EntityComponentsContainer.Clear();
                }
                else
                {
                    EntityComponentsContainer.Initialize(component.GetPlayableCharacter(), component.GetPresetSlot());
                }

            }
        }

        /// <summary>
        /// Trigged by an Event sent with a Button press on a party member gameObject.
        /// </summary>
        /// <param name="selectedItem">Selected party member</param>
        private void OnCharacterSelection_confirmed(GameObject selectedCharacter, UIActionSelection selection)
        {
            switch (selection)
            {
                case UIActionSelection.Primary:
                    SwapCharacterPositions(selectedCharacter);
                    break;
                case UIActionSelection.Secondary:
                    if (m_swapInProgress)
                    {

                    }
                    OpenCharacterActionsMenu(selectedCharacter);
                    break;
            };
        }
        #endregion

        public override void ExitPause()
        {
            base.ExitPause();
            m_playerInput.UI.Cancel.performed -= OnCancel_performed;
        }

        public override void Clear()
        {
        }

        protected override void UpdateInputActions()
        {
            m_inputActions = new Dictionary<string, string[]>()
            {
                { 
                    "Change Character", 
                    new string[]
                    { 
                        "UI_" + m_playerInput.UI.Navigate.name
                    }
                },
                { 
                    "Cycle Presets", 
                    new string[]
                    { 
                        "UI_" + m_playerInput.UI.SecondaryNavigate.name + ".horizontal"
                    }
                },
            };

            if(m_swapInProgress)
            {
                m_inputActions.Add("Validate Position",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Submit.name,
                        "UI_" + m_playerInput.UI.LeftClick.name
                    });
                m_inputActions.Add("Cancel Selection", 
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cancel.name
                    });
            }
            else
            {
                m_inputActions.Add("Select Character",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Submit.name,
                        "UI_" + m_playerInput.UI.LeftClick.name
                    });
                m_inputActions.Add("Exit Menu",
                    new string[] 
                    {
                        "UI_" + m_playerInput.UI.Cancel.name
                    });
            }
            base.UpdateInputActions();
        }

        public void NavigateToOtherMenu(int index)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "CharacterIndex", m_currentCharacter.GetComponent<UI_View_EntityInfos>().GetEntity().Id },
            };
            switch (index) {
                case 1: // Equipment
                    ChangeMenu(m_inventoryMenuIndex, parameters);
                    break;
                case 2: // Skills
                    ChangeMenu(m_skillsMenuIndex, parameters);
                    break;
                default:
                    throw new Exception("Unsupported navigation index");
            }
        }

        #region Private Methods
        private void SwapCharacterPositions(GameObject selectedCharacter)
        {
            if (!m_swapInProgress)
            {
                m_selectedCharacter = selectedCharacter;
                selectedCharacter.GetComponent<UI_View_EntityInfos>().ToggleCover();
                m_swapInProgress = true;
            }
            else
            {
                var char1 = PartyList.GetControlIndex(m_selectedCharacter);
                var char2 = PartyList.GetControlIndex(selectedCharacter);
                if (char1 != char2)
                {
                    m_partyManager.SwapCharactersPosition(char1, char2);
                    PartyList.Initialize(m_partyManager.GetActivePartyMembers(), m_partyManager.GetInactivePartyMembers(), m_partyManager.GetGuestCharacter());
                }
                else
                {
                    m_selectedCharacter.GetComponent<UI_View_EntityInfos>().ToggleCover();
                }
                m_swapInProgress = false;
            }
        }

        private void OpenCharacterActionsMenu(GameObject selectedCharacter)
        {
            PartyList.Deselect();
            SubActionMenu.SetActive(true);
            var position = selectedCharacter.transform.position;
            position.x += 250;
            SubActionMenu.transform.position = position;

            SubActionMenu.GetComponentsInChildren<Button>()[0].Select();
        }

        private void CancelSwapInProgress()
        {
            m_currentCharacter.GetComponent<UI_View_EntityInfos>().ToggleCover();
            m_swapInProgress = false;
        }

        private void CancelCurrentAction()
        {
            if (m_swapInProgress)
            {
                CancelSwapInProgress();
            }
            else if (m_ActionMenuOpened)
            {
                SubActionMenu.SetActive(false);
            }
            {
                ExitPause();
            }
        }
        #endregion
    }
}
