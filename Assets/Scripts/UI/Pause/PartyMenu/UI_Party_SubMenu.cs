using RPGTest.Enums;
using RPGTest.Managers;
using RPGTest.Models.Entity;
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
        [SerializeField] private GameObject[] PartyMemberWidgets;
        [SerializeField] private GameObject GuestMemberWidget;

        [SerializeField] private GameObject SubActionMenu;

        [SerializeField] private UI_PresetSlotSelector PresetSlotSelector;
        [SerializeField] private UI_View_EntityContainer EntityComponentContainer;

        [SerializeField] private TextMeshProUGUI Money;
        [SerializeField] private TextMeshProUGUI Location;

        //private int m_actualActiveMemberCount => ActivePartyMembersList.Where(m => m.GetComponent<UI_Member_Widget>().GetCharacter() != null).Count() -1;
        //private int m_actualInactiveMemberCount => InactivePartyMembersList.Where(m => m.GetComponent<UI_Member_Widget>().GetCharacter() != null).Count();
      
        //Items control
        private int m_currentNavigationIndex = 0;
        private int m_inactiveMembersPerRow = 4;
        private int m_activePartyMemberCount => m_partyManager.GetActivePartyThreshold();
        private int m_maxRegularNavigationIndex => m_partyManager.GetIndexofLastExistingPartyMember() < PartyMemberWidgets.Count() ? m_partyManager.GetIndexofLastExistingPartyMember() : PartyMemberWidgets.Count() - 1;

        // Swap controls
        private GameObject m_selectedCharacterGo = null;
        private bool m_swapInProgress = false;

        // SubMenu controls
        private bool m_ActionMenuOpened = false;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        // TODO : get rid of that, use the event system
        private PlayableCharacter m_currentCharacter => PartyMemberWidgets[m_currentNavigationIndex].GetComponent<UI_View_EntityInfos>().GetPlayableCharacter();

        private int m_inventoryMenuIndex = 1; // todo : need to keep that const elsewhere
        private int m_skillsMenuIndex = 2; // todo: same as line above

        public override void Awake()
        {
            base.Awake();
            m_playerInput.UI.Navigate.started += ctx => { 
                m_navigateStarted = true;
            };
            m_playerInput.UI.Navigate.performed += ctx => 
            {
                if (m_ActionMenuOpened) return;
                m_performTimeStamp = Time.time + 0.3f;
                Navigate_Performed(ctx.ReadValue<Vector2>());
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
                Submit_Performed();
            };
            m_playerInput.UI.SecondaryAction.performed += ctx =>
            {
                SecondaryAction_Performed();
            };
            m_playerInput.UI.MouseMoved.performed += ctx =>
            {
                MouseMoved_Performed();
            };
            m_playerInput.UI.RightClick.performed += ctx =>
            {
                MouseRightClick_Performed();
            };
        }

        public override void OnEnable()
        {
            base.OnEnable();
            m_playerInput.Player.Debug.performed += Debug_performed;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            m_playerInput.Player.Debug.performed -= Debug_performed;
        }

        public override void Initialize()
        {
            UpdateCharacterControls();
            Money.text = FindObjectOfType<InventoryManager>().Money.ToString();
            Location.text = SceneManager.GetActiveScene().name;
        }

        public override void OpenSubMenu(Dictionary<string, object> parameters)
        {
            base.OpenSubMenu(parameters);
            m_playerInput.UI.Cancel.performed += OnCancel_performed;

            SubActionMenu.SetActive(false);
            PartyMemberWidgets.First(w => w.GetComponent<UI_View_EntityInfos>().GetEntity() != null).GetComponent<Button>().Select();
            m_currentNavigationIndex = 0;
            InitializeEntityInformations();
            UpdateInputActions();
        }

        public void Start()
        {
            foreach(var widget in PartyMemberWidgets)
            {
                var widgetScript = widget.GetComponent<UI_View_EntityInfos>();
                widgetScript.MemberSelected += OnMember_Selected;
                widgetScript.SetSecondarySelect(() => widgetScript.ToggleCover());
            }
        }

        public void Update()
        {
            if (m_navigateStarted && (Time.time - m_performTimeStamp) >= WaitTimeBetweenPerforms)
            {
                m_performTimeStamp = Time.time;
                Navigate_Performed(m_playerInput.UI.Navigate.ReadValue<Vector2>());
            }
        }

        #region Input Events
        // Select character
        private void Submit_Performed()
        {
            if (m_currentNavigationIndex == -1) return;
            PartyMemberWidgets[m_currentNavigationIndex].GetComponent<UI_View_EntityInfos>().ToggleCover();
            SwapCharacterPositions(PartyMemberWidgets[m_currentNavigationIndex]);
        }

        // Open Action Menus 
        private void SecondaryAction_Performed()
        {
            OpenCharacterActionsMenu(PartyMemberWidgets[m_currentNavigationIndex]);
        }

        // Cancel current action (swap or sub menu)
        protected override void OnCancel_performed(InputAction.CallbackContext obj)
        {
            CancelCurrentAction();
        }

        // Change character
        private void Navigate_Performed(Vector2 movement)
        {
            if (m_currentNavigationIndex == -1)
            {
                m_currentNavigationIndex = 0;
            }

            if (movement.y > 0.4f)
            {
                NavigateUp();
            }
            else if (movement.y < -0.4f)
            {
                NavigateDown();
            }
            else if (movement.x < -0.4f)
            {
                NavigateLeft();
            }
            else if(movement.x > 0.04f)
            {
                NavigateRight();
            }
            PartyMemberWidgets[m_currentNavigationIndex].GetComponent<Button>().Select();
            RefreshEntityComponentContainer();
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
            m_currentNavigationIndex = -1;
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
        /// Trigged by an Event sent with a Button press on a party member gameObject.
        /// </summary>
        /// <param name="selectedItem">Selected party member</param>
        private void OnMember_Selected(UIActionSelection selection, GameObject selectedCharacter)
        {
            switch(selection)
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
                { "CharacterIndex", PartyMemberWidgets[m_currentNavigationIndex].GetComponent<UI_View_EntityInfos>().GetEntity().Id },
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
        private void NavigateDown()
        {
            int maxIndex = m_swapInProgress ? GetLastPossibleIndex() : m_maxRegularNavigationIndex;
            if (m_currentNavigationIndex < m_activePartyMemberCount)
            {
                m_currentNavigationIndex += 1;
            }
            else if (m_currentNavigationIndex >= m_activePartyMemberCount)
            {
                if(m_currentNavigationIndex - m_activePartyMemberCount < m_inactiveMembersPerRow)
                {
                    if( m_currentNavigationIndex + m_inactiveMembersPerRow > maxIndex)
                    {
                        m_currentNavigationIndex = maxIndex;
                    }
                    else
                    {
                        m_currentNavigationIndex += m_inactiveMembersPerRow;
                    }
                }
            }
        }

        private void NavigateUp()
        {
            // In the active members list
            if (m_currentNavigationIndex < m_activePartyMemberCount && m_currentNavigationIndex > 0)
            {
                m_currentNavigationIndex -= 1;
            }
            else if(m_currentNavigationIndex >= m_activePartyMemberCount && m_currentNavigationIndex - m_activePartyMemberCount < m_inactiveMembersPerRow)
            {
                m_currentNavigationIndex = m_activePartyMemberCount - 1;
            }
            else if(m_currentNavigationIndex >= m_activePartyMemberCount + m_inactiveMembersPerRow)
            {
                m_currentNavigationIndex -= m_inactiveMembersPerRow;
            }
        }

        private void NavigateLeft()
        {
            int remainder = (m_currentNavigationIndex - m_activePartyMemberCount) % m_inactiveMembersPerRow;

            if (m_currentNavigationIndex > m_activePartyMemberCount && remainder != 0)
            {
                m_currentNavigationIndex -= 1;
            }
        }

        private void NavigateRight()
        {
            int maxIndex = m_swapInProgress ? GetLastPossibleIndex() : m_maxRegularNavigationIndex;
            if (m_currentNavigationIndex >= m_activePartyMemberCount
                && m_currentNavigationIndex < maxIndex
                && m_currentNavigationIndex - m_activePartyMemberCount != m_inactiveMembersPerRow - 1)
            {
                m_currentNavigationIndex += 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateCharacterControls()
        {
            var characters = m_partyManager.GetAllPartyMembers();
            if (PartyMemberWidgets.Count() != characters.Count)
            {
                Debug.LogError($"Missmatch! widgets : {PartyMemberWidgets.Count()} != characters : {characters.Count}");
                return;
            }

            for (int index = 0; index < characters.Count; index++)
            {
                Entity character = characters[index];
                PartyMemberWidgets[index].GetComponent<UI_View_EntityInfos>().Initialize(character, Models.PresetSlot.First);
            }

            GuestMemberWidget.GetComponent<UI_View_EntityInfos>().Initialize(m_partyManager.GetGuestCharacter(), Models.PresetSlot.First);
        }

        private void Refresh()
        {
            UpdateCharacterControls();
        }

        public void InitializeEntityInformations()
        {
            EntityComponentContainer.Initialize(m_currentCharacter, PresetSlotSelector.GetCurrentPreset());
        }

        private void RefreshEntityComponentContainer()
        {
            if (EntityComponentContainer != null)
            {
                // TODO : preset
                // EntityInfosView.Refresh(m_currentCharacter);
            }
        }

        private void SwapCharacterPositions(GameObject selectedCharacter)
        {
            if (!m_swapInProgress)
            {
                m_selectedCharacterGo = selectedCharacter;
                m_swapInProgress = true;
            }
            else
            {
                var initialIndex = Array.FindIndex(PartyMemberWidgets, widget => widget == m_selectedCharacterGo);
                var otherIndex = Array.FindIndex(PartyMemberWidgets, widget => widget == selectedCharacter);
                
                if (initialIndex != otherIndex)
                {
                    m_partyManager.PerformSwap(initialIndex, otherIndex);
                    FindAndFixHoles(m_activePartyMemberCount);

                    Refresh();
                    RefreshEntityComponentContainer();
                }
                m_swapInProgress = false;
            }
        }

        private void OpenCharacterActionsMenu(GameObject selectedCharacter)
        {
            m_ActionMenuOpened = true;
            SubActionMenu.SetActive(true);
            var position = selectedCharacter.transform.position;
            position.x += 250;
            SubActionMenu.transform.position = position;

            SubActionMenu.GetComponentsInChildren<Button>()[0].Select();
        }

        private void CancelSwapInProgress()
        {
            m_selectedCharacterGo.GetComponent<UI_View_EntityInfos>().ToggleCover();
            m_selectedCharacterGo = null;
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

        // Retrieve the first empty index.
        private int GetLastPossibleIndex()
        {
            var index = m_partyManager.GetIndexofLastExistingPartyMember();
            if (index < PartyMemberWidgets.Length - 1)
            {
                index++;
                return index;
            }
            return index;
        }

        // Fix any potential holes between 2 members after a swap
        private void FindAndFixHoles(int startIndex)
        {
            var firstEmptyIndex = -1;
            for (int i = startIndex; i < m_partyManager.GetAllPartyMembers().Count; i++)
            {
                m_partyManager.TryGetPartyMemberAtIndex(i, out var member);
                if (member == null && firstEmptyIndex == -1)
                {
                    firstEmptyIndex = i;
                } else if(member != null && firstEmptyIndex != -1)
                {
                    m_partyManager.PerformSwap(firstEmptyIndex, i);
                    i = firstEmptyIndex++;
                    firstEmptyIndex = -1;
                }
            }
        }
        #endregion
    }
}
