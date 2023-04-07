﻿using RPGTest.Managers;
using RPGTest.Models.Entity;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPGTest.UI.PartyMenu
{
    public class UI_SubMenu_Party : UI_Pause_SubMenu
    {
        [SerializeField] private GameObject[] PartyMemberWidgets;
        [SerializeField] private GameObject GuestMemberWidget;
        [SerializeField] private UI_MemberDetails_Widget StatsWidget;
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
        private int m_indexToSwap = 0;
        private int m_maxSwappingIndex = 0;
        private bool m_swapInProgress = false;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        private PlayableCharacter m_currentCharacter => PartyMemberWidgets[m_currentNavigationIndex].GetComponent<UI_Member_Widget>().GetCharacter();

        // Hold navigation control
        private bool m_navigateStarted = false;
        public float WaitTimeBetweenPerforms = 0.4f;
        private float m_performTimeStamp;

        public override void Awake()
        {
            base.Awake();
            m_playerInput.UI.Navigate.started += ctx => { 
                m_navigateStarted = true;
                UpdateIconDisplay(ctx.control.device, GetActions());
            };
            m_playerInput.UI.Navigate.performed += ctx => 
            {
                m_performTimeStamp = Time.time + 0.3f;
                Navigate(ctx.ReadValue<Vector2>());
                UpdateIconDisplay(ctx.control.device, GetActions());
            };
            m_playerInput.UI.SecondaryNavigate.performed += ctx =>
            {
                SecondaryNavigate_Performed(ctx.ReadValue<Vector2>());
                UpdateIconDisplay(ctx.control.device, GetActions());
            };
            m_playerInput.UI.Navigate.canceled += ctx => { 
                m_navigateStarted = false;
                UpdateIconDisplay(ctx.control.device, GetActions());
            };
        }

        public void Start()
        {
            foreach(var widget in PartyMemberWidgets)
            {
                var widgetScript = widget.GetComponent<UI_Member_Widget>();
                widgetScript.MemberSelected += SwapMembers;
                widgetScript.SetSecondarySelect(() => widgetScript.ToggleCover());
            }
        }

        public void Update()
        {
            if (m_navigateStarted && (Time.time - m_performTimeStamp) >= WaitTimeBetweenPerforms)
            {
                m_performTimeStamp = Time.time;
                Navigate(m_playerInput.UI.Navigate.ReadValue<Vector2>());
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            m_playerInput.Player.Debug.performed += Debug_performed;
        }

        public override void OnDisable() {
            base.OnDisable();
            m_playerInput.Player.Debug.performed -= Debug_performed;
        }

        #region Input Events
        protected override void Cancel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(m_swapInProgress)
            {
                PartyMemberWidgets[m_indexToSwap].GetComponent<UI_Member_Widget>().ToggleCover();
                m_swapInProgress = false;
            }
            else
            {
                CloseMenu();
            }
        }

        public void Navigate(Vector2 movement)
        {
            if (IsSubMenuSelected)
            {
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
                RefreshDetailsPanel();
            }
        }

        private void SecondaryNavigate_Performed(Vector2 movement)
        {
            if (StatsWidget != null && (movement.x < -0.4f || movement.x > 0.04f))
            {
                StatsWidget.ChangePreset(m_currentCharacter);
            }
        }

        private void Debug_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
        }
        #endregion

        public override void OpenMenu(InputDevice device)
        {
            base.OpenMenu(device);
            m_playerInput.UI.Cancel.performed += Cancel_performed;

            PartyMemberWidgets.First(w => w.GetComponent<UI_Member_Widget>().GetCharacter() != null).GetComponent<Button>().Select();
            m_currentNavigationIndex = 0;
            if(StatsWidget != null)
            {
                RefreshDetailsPanel();
            }
            UpdateIconDisplay(device, GetActions());
        }

        public override void CloseMenu()
        {
            base.CloseMenu();
            m_playerInput.UI.Cancel.performed -= Cancel_performed;
        }

        public override void Initialize(bool refreshAll = true)
        {
            if (refreshAll)
            {
                m_indexToSwap = 0; 
            }

            InitializeWidgets();
            Money.text = FindObjectOfType<InventoryManager>().Money.ToString();
            Location.text = SceneManager.GetActiveScene().name;
        }

        public override void Clear()
        {
        }

        protected override Dictionary<string, string> GetActions()
        {

            var actions = new Dictionary<string, string>()
            {
                { "UI_" + m_playerInput.UI.Navigate.name, "Change Character" },
                { "UI_" + m_playerInput.UI.SecondaryNavigate.name + ".horizontal", "Cycle Presets" },
            };

            if(m_swapInProgress)
            {
                actions.Add("UI_" + m_playerInput.UI.Submit.name, "Validate Position");
                actions.Add("UI_" + m_playerInput.UI.Cancel.name, "Cancel Selection");
            } else
            {
                actions.Add("UI_" + m_playerInput.UI.Submit.name, "Validate Position");
                actions.Add("UI_" + m_playerInput.UI.Cancel.name, "Exit Menu");
            }
            Debug.Log(m_playerInput.UI.Submit.name);


            return actions;
        }

        #region Private Methods
        private void NavigateDown()
        {
            int maxIndex = m_swapInProgress ? m_maxSwappingIndex : m_maxRegularNavigationIndex;
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
            int maxIndex = m_swapInProgress ? m_maxSwappingIndex : m_maxRegularNavigationIndex;
            if (m_currentNavigationIndex >= m_activePartyMemberCount
                && m_currentNavigationIndex < maxIndex
                && m_currentNavigationIndex - m_activePartyMemberCount != m_inactiveMembersPerRow - 1)
            {
                m_currentNavigationIndex += 1;
            }
        }

        private void InitializeWidgets()
        {
            var characters = m_partyManager.GetAllPartyMembers();
            if (PartyMemberWidgets.Count() != characters.Count)
            {
                Debug.LogError($"Missmatch! widgets : {PartyMemberWidgets.Count()} != characters : {characters.Count}");
                return;
            }

            for (int index = 0; index < characters.Count; index++)
            {
                PlayableCharacter character = characters[index];
                PartyMemberWidgets[index].GetComponent<UI_Member_Widget>().Initialize(character);
            }

            GuestMemberWidget.GetComponent<UI_Member_Widget>().Initialize(m_partyManager.GetGuestCharacter());
        }

        /// <summary>
        /// Trigged by an Event sent with a Button press on a party member.
        /// </summary>
        /// <param name="selectedItem">Selected party member</param>
        private void SwapMembers(GameObject selectedItem)
        {
            if(!m_swapInProgress)
            {
                m_indexToSwap = m_currentNavigationIndex;
                if (m_currentNavigationIndex < m_activePartyMemberCount)
                {
                    m_maxSwappingIndex =  m_partyManager.GetExistingActivePartyMembers().Count() > 1 ? m_maxRegularNavigationIndex + 1 : m_maxRegularNavigationIndex;
                }
                else
                {
                    m_maxSwappingIndex = m_maxRegularNavigationIndex;
                }

                m_swapInProgress = true;
            }
            else
            {
                var indexToSwapWith = m_currentNavigationIndex;
                if (indexToSwapWith != m_indexToSwap)
                {
                    m_partyManager.PerformSwap(m_indexToSwap, indexToSwapWith);
                    FindAndFixHoles();


                    Refresh();
                    RefreshDetailsPanel();
                }
                m_swapInProgress = false;
            }
        }

        private void Refresh()
        {
            Initialize(false);
        }

        private void RefreshDetailsPanel()
        {
            if (StatsWidget != null)
            {
                StatsWidget.Refresh(m_currentCharacter);
            }
        }

        // Fix any potential holes between 2 members after a swap
        private void FindAndFixHoles()
        {
            bool foundEmptyIndex = false;
            for (int i = m_activePartyMemberCount; i < m_partyManager.GetAllPartyMembers().Count; i++)
            { 
                if (foundEmptyIndex)
                {
                    m_partyManager.PerformSwap(i, i -1);
                }

                PlayableCharacter member = m_partyManager.GetPartyMemberAtIndex(i);
                if (member == null)
                {
                    foundEmptyIndex = true;
                    continue;
                }

                foundEmptyIndex = false;
            }
        }
        #endregion
    }
}
