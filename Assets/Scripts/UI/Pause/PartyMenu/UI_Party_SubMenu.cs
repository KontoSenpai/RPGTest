using RPGTest.Enums;
using RPGTest.Managers;
using RPGTest.Models.Entity;
using RPGTest.Modules.Party;
using RPGTest.UI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace RPGTest.UI.PartyMenu
{
    public enum ActionStage
    {
        SelectSlot,
        SelectSwap,
        SelectSecondaryAction,
    }

    public enum SubActionChoices
    {
        [Name("Go To Equpiment Page")]
        NavigateEquipment,
        [Name("Go To Skill Page")]
        NavigateSkills,
        [Name("Remove From Party")]
        RemoveFromParty,
        [Name("Add To Party")]
        AddToParty,
    }

    public class UI_Party_SubMenu : UI_Pause_SubMenu
    {
        [SerializeField] private UI_PartyList PartyList;

        [SerializeField] private UI_View_EntityContainer EntityComponentsContainer;

        [SerializeField] private UI_View_ActionSelection SubActionSelectionDialog;

        [SerializeField] private UI_PresetSlotSelector PresetSlotSelector;

        [SerializeField] private TextMeshProUGUI Money;
        [SerializeField] private TextMeshProUGUI Location;

        private ActionStage m_currentActionStage = ActionStage.SelectSlot;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        private int m_inventoryMenuIndex = 1; // todo : need to keep that const elsewhere
        private int m_skillsMenuIndex = 2; // todo: same as line above

        private GameObject m_currentCharacter;
        private GameObject m_selectedCharacter;

        public override void Awake()
        {
            base.Awake();

            PartyList.SecondaryActionPerformed += OnSecondaryAction_performed;
            SubActionSelectionDialog.CharacterEnumActionSelected += SubActionSelectionDialog_EnumActionSelected;
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

            ChangeStage(ActionStage.SelectSlot);
        }

        public override void OpenSubMenu(Dictionary<string, object> parameters)
        {
            base.OpenSubMenu(parameters);
            m_playerInput.UI.Cancel.performed += OnCancel_performed;

            SubActionSelectionDialog.gameObject.SetActive(false);
            PartyList.Select(m_currentCharacter);
            UpdateInputActions();
        }

        public override void CloseSubMenu()
        {
            m_playerInput.UI.Cancel.performed -= OnCancel_performed;
            m_currentCharacter = null;
            base.CloseSubMenu();
        }

        public override void ExitPause()
        {
            m_playerInput.UI.Cancel.performed -= OnCancel_performed;
            m_currentCharacter = null;
            base.ExitPause();
        }

        protected override void UpdateInputActions()
        {
            if (!this.isActiveAndEnabled)
            {
                return;
            }
            m_inputActions = new Dictionary<string, string[]>();
            switch (m_currentActionStage)
            {
                case ActionStage.SelectSlot:
                    foreach (var input in PartyList.GetInputDisplay(m_playerInput))
                    {
                        m_inputActions.Add(input.Key, input.Value);
                    }

                    m_inputActions.Add("Exit",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cancel.name,
                    });
                    break;
                case ActionStage.SelectSwap:
                    foreach (var input in PartyList.GetInputDisplay(m_playerInput))
                    {
                        m_inputActions.Add(input.Key, input.Value);
                    }

                    m_inputActions.Add("Cancel",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cancel.name,
                    });
                    break;
                case ActionStage.SelectSecondaryAction:
                    foreach (var input in SubActionSelectionDialog.GetInputDisplay(m_playerInput))
                    {
                        m_inputActions.Add(input.Key, input.Value);
                    }
                    m_inputActions.Add("Cancel", new string[]
                    {
                        "UI_" + m_playerInput.UI.Cancel.name,
                    });
                    break;
            }

            base.UpdateInputActions();
        }

        public void NavigateToOtherMenu(PlayableCharacter character, int index)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "CharacterId", character.Id },
            };
            switch (index)
            {
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

        #region Input Events
        // Cancel current action (swap or sub menu)
        protected override void OnCancel_performed(InputAction.CallbackContext obj)
        {
            CancelCurrentAction();
        }

        // Deselect Button
        private void MouseMoved_Performed()
        {
            FindObjectOfType<EventSystem>().SetSelectedGameObject(null);
        }

        public void MouseRightClick_Performed()
        {
            if (m_currentActionStage == ActionStage.SelectSwap)
            {
                ChangeStage(ActionStage.SelectSlot);
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
                SubActionSelectionDialog.gameObject.SetActive(false);
            }
            else
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
                m_currentCharacter = selectedCharacter;
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
                    if (m_currentActionStage == ActionStage.SelectSlot)
                    {
                        SaveSelectedCharacter(selectedCharacter);
                    } else if (m_currentActionStage == ActionStage.SelectSwap)
                    {
                        SwapCharacterPositions(selectedCharacter);
                    }
                    break;
                case UIActionSelection.Secondary:
                    if (m_currentActionStage == ActionStage.SelectSwap)
                    {
                        return;
                    }
                    OpenCharacterActionsMenu(selectedCharacter);
                    break;
            };
        }

        private void SubActionSelectionDialog_EnumActionSelected(PlayableCharacter character, Enum enumAction)
        {
            var char1 = PartyList.GetControlIndex(m_currentCharacter);
            switch (enumAction)
            {
                case SubActionChoices.NavigateEquipment:
                    NavigateToOtherMenu(character, m_inventoryMenuIndex);
                    break;
                case SubActionChoices.NavigateSkills:
                    NavigateToOtherMenu(character, m_skillsMenuIndex);
                    break;
                case SubActionChoices.AddToParty:
                    m_partyManager.SwapCharactersPosition(char1, m_partyManager.GetIndexOfFirstEmptyActiveSlot());
                    ChangeStage(ActionStage.SelectSlot);
                    break;
                case SubActionChoices.RemoveFromParty:
                    m_partyManager.SwapCharactersPosition(char1, m_partyManager.GetIndexOfFirstEmptyInactiveSlot());
                    ChangeStage(ActionStage.SelectSlot);
                    break;
            }
        }
        #endregion

        #region Private Methods
        private void ChangeStage(ActionStage stage)
        {
            m_currentActionStage = stage;
            switch (stage)
            {
                case ActionStage.SelectSlot:
                    SubActionSelectionDialog.gameObject.SetActive(false);
                    PartyList.Initialize(m_partyManager.GetActivePartyMembers(), m_partyManager.GetInactivePartyMembers(), m_partyManager.GetGuestCharacter());
                    PartyList.Select(m_currentCharacter);
                    break;
                case ActionStage.SelectSwap:
                    m_selectedCharacter.GetComponent<UI_View_EntityInfos>().ToggleCover();
                    PartyList.DisableSecondaryAction();
                    PartyList.RefreshNavigation(m_partyManager.GetExistingActivePartyMembers().Count > 1);
                    break;
                case ActionStage.SelectSecondaryAction:
                    PartyList.Deselect();
                    SubActionSelectionDialog.gameObject.SetActive(true);
                    break;
            }

            UpdateInputActions();
        }

        private void SaveSelectedCharacter(GameObject selectedCharacter)
        {
            m_selectedCharacter = selectedCharacter;
            ChangeStage(ActionStage.SelectSwap);
        }

        private void SwapCharacterPositions(GameObject selectedCharacter)
        {
            var char1 = PartyList.GetControlIndex(m_selectedCharacter);
            var char2 = PartyList.GetControlIndex(selectedCharacter);
            if (char1 != char2)
            {
                m_partyManager.SwapCharactersPosition(char1, char2);
            }

            ChangeStage(ActionStage.SelectSlot);
        }

        private void OpenCharacterActionsMenu(GameObject selectedCharacter)
        {
            ChangeStage(ActionStage.SelectSecondaryAction);
            var actions = new List<Enum> { SubActionChoices.NavigateEquipment, SubActionChoices.NavigateSkills };

            var character = selectedCharacter.GetComponent<UI_View_EntityInfos>().GetPlayableCharacter();

            if (character != null
                && m_partyManager.GetActivePartyMembers().Any(c => c != null && c.Id == character.Id)
                && m_partyManager.GetExistingActivePartyMembers().Count > 1)
            {
                actions.Add(SubActionChoices.RemoveFromParty);
            } else if (character != null
                && m_partyManager.GetInactivePartyMembers().Any(c => c != null && c.Id == character.Id)
                && m_partyManager.GetExistingActivePartyMembers().Count < m_partyManager.GetActivePartyThreshold())
            {
                actions.Add(SubActionChoices.AddToParty);
            }
            
            SubActionSelectionDialog.Initialize(selectedCharacter, actions);
        }

        private void CancelCurrentAction()
        {
            switch (m_currentActionStage)
            {
                case ActionStage.SelectSlot:
                    ExitPause();
                    break;
                case ActionStage.SelectSwap:
                    ChangeStage(ActionStage.SelectSlot);
                    m_currentCharacter = null;
                    break;
                case ActionStage.SelectSecondaryAction:
                    ChangeStage(ActionStage.SelectSlot);
                    break;
            }
        }
        #endregion
    }
}
