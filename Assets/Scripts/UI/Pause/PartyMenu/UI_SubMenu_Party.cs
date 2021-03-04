using RPGTest.Helpers;
using RPGTest.Managers;
using RPGTest.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPGTest.UI.PartyMenu
{
    public class UI_SubMenu_Party : UI_Pause_SubMenu
    {
        [SerializeField] private GameObject[] PartyMemberWidgets;
        [SerializeField] private UI_MemberDetails_Widget StatsWidget;
        [SerializeField] private TextMeshProUGUI Money;
        [SerializeField] private TextMeshProUGUI Location;

        //private int m_actualActiveMemberCount => ActivePartyMembersList.Where(m => m.GetComponent<UI_Member_Widget>().GetCharacter() != null).Count() -1;
        //private int m_actualInactiveMemberCount => InactivePartyMembersList.Where(m => m.GetComponent<UI_Member_Widget>().GetCharacter() != null).Count();

        
        //Items control
        private int m_currentNavigationIndex = 0;
        private int m_indexToSwap = 0;
        private int m_inactiveMembersPerRow = 4;

        private int m_activePartyMemberCount => m_partyManager.GetActivePartyThreshold();

        private bool m_swapInProgress = false;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        public override void Awake()
        {
            base.Awake();
            m_playerInput.UI.Navigate.performed += Navigate_performed;
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

        public override void OnEnable()
        {
            base.OnEnable();
            m_playerInput.UI.Cancel.performed += Cancel_performed;
            m_playerInput.Player.Debug.performed += Debug_performed;
        }

        private void Debug_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
        }

        public override void OnDisable() => m_playerInput.Disable();


        protected override void Cancel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(m_swapInProgress)
            {
                //m_allMembers[m_indexToSwap].GetComponent<UI_Member_Widget>().Select();
            }
            else
            {
                CloseMenu();
            }
        }

        private void Navigate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(IsSubMenuSelected)
            {
                var movement = obj.ReadValue<Vector2>();
                //Moving up
                if (movement.y > 0.4f)
                {
                    // In the active members list
                    if(m_currentNavigationIndex < m_activePartyMemberCount && m_currentNavigationIndex > 0)
                    {
                        m_currentNavigationIndex -= 1;
                    }
                    // In the first row of the inactive member list
                    else if(m_currentNavigationIndex >= m_activePartyMemberCount && (m_currentNavigationIndex - m_activePartyMemberCount) < m_inactiveMembersPerRow)
                    {
                        m_currentNavigationIndex = GetLastActivePartyMemberIndex(true);
                    }
                    // In subsequent rows
                    else if(m_currentNavigationIndex >= m_activePartyMemberCount && (m_currentNavigationIndex - m_activePartyMemberCount) >= m_inactiveMembersPerRow)
                    {
                        m_currentNavigationIndex -= m_inactiveMembersPerRow;
                    }
                }
                //Moving down
                else if (movement.y < -0.4f)
                {
                    if( m_currentNavigationIndex < m_activePartyMemberCount -1 && PartyMemberWidgets[m_currentNavigationIndex + 1].GetComponent<UI_Member_Widget>().GetCharacter() != null)
                    {
                        m_currentNavigationIndex += 1;
                    }
                    else if(m_currentNavigationIndex == m_activePartyMemberCount - 1 && GetInactivePartyMembersCount() > 0)
                    {
                        m_currentNavigationIndex += 1;
                    }
                    else if(m_currentNavigationIndex >= m_activePartyMemberCount && GetInactivePartyMembersCount() > m_inactiveMembersPerRow)
                    {
                        int inactiveIndex = m_currentNavigationIndex - m_activePartyMemberCount - 1;
                        if(inactiveIndex < m_inactiveMembersPerRow)
                        {
                            if(PartyMemberWidgets[m_currentNavigationIndex + m_inactiveMembersPerRow].GetComponent<UI_Member_Widget>().GetCharacter() != null)
                            {
                                m_currentNavigationIndex += m_inactiveMembersPerRow;
                            }
                            else
                            {
                                int newIndex = Array.IndexOf(PartyMemberWidgets, PartyMemberWidgets.Last(w => w.GetComponent<UI_Member_Widget>().GetCharacter() != null));
                                if(newIndex > m_currentNavigationIndex)
                                {
                                    m_currentNavigationIndex += (newIndex - m_currentNavigationIndex);
                                }
                            }
                        }
                    }
                }
                
                PartyMemberWidgets[m_currentNavigationIndex].GetComponent<Button>().Select();
                RefreshDetailsPanel();
            }
        }

        public override void OpenMenu()
        {
            base.OpenMenu();
            m_playerInput.UI.Cancel.performed += Cancel_performed;

            PartyMemberWidgets.First(w => w.GetComponent<UI_Member_Widget>().GetCharacter() != null).GetComponent<Button>().Select();
            if(StatsWidget != null)
            {
                StatsWidget.Open(true);
                RefreshDetailsPanel();
            }
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

            InitializeWidgets(PartyMemberWidgets, m_partyManager.GetAllPartyMembers());
            /*
Button up = null;
Button down = null;
Button left = null;
Button right = null;

//Expliciting navigation
List<GameObject> existingActiveMembers = ActivePartyMembersList.Where(m => m.GetComponent<UI_Member_Widget>().GetCharacter() != null).ToList();
for(int index = 0; index < existingActiveMembers.Count; index++)
{
    up = null;
    down = null;
    if (index > 0)
    {
        up = existingActiveMembers[index - 1].GetComponent<Button>();
    }
    if (index < m_actualActiveMemberCount)
    {
        down = existingActiveMembers[index + 1].GetComponent<Button>();
    }
    else if (m_actualInactiveMemberCount > 0)
    {
        down = InactivePartyMembersList[0].GetComponent<Button>();
    }
    existingActiveMembers[index].GetComponent<Button>().ExplicitNavigation(Up : up, Down: down);
}

List<GameObject> existingInactiveMembers = InactivePartyMembersList.Where(m => m.GetComponent<UI_Member_Widget>().GetCharacter() != null).ToList();
for(int index = 0; index < existingInactiveMembers.Count; index++)
{
    up = null;
    down = null;
    left = null;
    right = null;

    //Ups
    if(index < InactiveMembersPerRow)
    {
        up = existingActiveMembers.Last().GetComponent<Button>();
    }
    else
    {
        up = existingInactiveMembers[index - InactiveMembersPerRow].GetComponent<Button>(); 
    }
    //Downs
    if(index < InactiveMembersPerRow && existingInactiveMembers.Count > InactiveMembersPerRow)
    {
        if(index + InactiveMembersPerRow < existingInactiveMembers.Count)
        {
            down = existingInactiveMembers[index + InactiveMembersPerRow].GetComponent<Button>();
        }
        else
        {
            down = existingInactiveMembers[InactiveMembersPerRow].GetComponent<Button>();
        }
    }
    // Lefts
    if(index > 0)
    {
        left = existingInactiveMembers[index - 1].GetComponent<Button>();
    }
    // Rights
    if(index < existingInactiveMembers.Count -1)
    {
        right = existingInactiveMembers[index + 1].GetComponent<Button>();
    }
    existingInactiveMembers[index].GetComponent<Button>().ExplicitNavigation(Left: left, Right: right, Up: up, Down: down);
}
*/
            Money.text = FindObjectOfType<InventoryManager>().Money.ToString();
            Location.text = SceneManager.GetActiveScene().name;
        }

        public override void Clear()
        {
        }

        #region Private Methods
        private int GetLastActivePartyMemberIndex(bool exists)
        {
            if(exists)
            {
                var filteredMembers = PartyMemberWidgets.Where(w => Array.IndexOf(PartyMemberWidgets, w) < m_activePartyMemberCount).Select(w => w);
                return Array.IndexOf(PartyMemberWidgets, filteredMembers.Last(w => w.GetComponent<UI_Member_Widget>().GetCharacter() != null));
            }
            {
                return m_activePartyMemberCount - 1;
            }
        }

        private int GetInactivePartyMembersCount()
        {
            return PartyMemberWidgets.Skip(m_activePartyMemberCount).Select(w => w.GetComponent<UI_Member_Widget>().GetCharacter() != null).Count();
        }

        private void InitializeWidgets(GameObject[] widgets, List<PlayableCharacter> characters)
        {
            if (widgets.Count() != characters.Count)
            {
                Debug.LogError($"Missmatch! widgets : {widgets.Count()} != characters : {characters.Count}");
                return;
            }

            for (int index = 0; index < characters.Count; index++)
            {
                PlayableCharacter character = characters[index];
                widgets[index].GetComponent<UI_Member_Widget>().Initialize(character);
            }
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
                m_swapInProgress = true;
            }
            else
            {
                var indexToSwapWith = m_currentNavigationIndex;
                if (indexToSwapWith != m_indexToSwap)
                {
                    m_partyManager.PerformSwap(m_indexToSwap, indexToSwapWith);
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
            PlayableCharacter character = PartyMemberWidgets[m_currentNavigationIndex].GetComponent<UI_Member_Widget>().GetCharacter();
            if (StatsWidget != null && character != null)
            {
                StatsWidget.Refresh(character);
            }
        }
        #endregion
    }
}
