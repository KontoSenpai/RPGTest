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
        private bool m_navigateStarted = false;
        
        public float WaitTimeBetweenPerforms = 0.4f;
        private float m_performTimeStamp;

        public override void Awake()
        {
            base.Awake();
            m_playerInput.UI.Navigate.started += ctx => { m_navigateStarted = true; };
            m_playerInput.UI.Navigate.performed += ctx => 
            {
                m_performTimeStamp = Time.time + 0.3f;
                Navigate(ctx.ReadValue<Vector2>());
            };
            m_playerInput.UI.Navigate.canceled += ctx => { m_navigateStarted = false; };
            m_playerInput.UI.SubAction1.performed += SubAction1_performed;
        }

        private void SubAction1_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(StatsWidget != null)
            {
                StatsWidget.SwapDisplay();
            }
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
            m_playerInput.UI.Cancel.performed += Cancel_performed;
            m_playerInput.Player.Debug.performed += Debug_performed;
        }

        private void Debug_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
        }

        public override void OnDisable() => m_playerInput.Disable();

        #region Input Events
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
        #endregion

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
        private void NavigateDown()
        {
            if (m_currentNavigationIndex < m_activePartyMemberCount)
            {
                m_currentNavigationIndex += 1;
            }
            else if (m_currentNavigationIndex >= m_activePartyMemberCount)
            {
                if(m_currentNavigationIndex - m_activePartyMemberCount < m_inactiveMembersPerRow)
                {
                    m_currentNavigationIndex += m_inactiveMembersPerRow;
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
            if (m_currentNavigationIndex >= m_activePartyMemberCount && m_currentNavigationIndex < PartyMemberWidgets.Count() - 1 && m_currentNavigationIndex - m_activePartyMemberCount != m_inactiveMembersPerRow - 1)
            {
                m_currentNavigationIndex += 1;
            }
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
            if (StatsWidget != null)
            {
                StatsWidget.Refresh(character);
            }
        }
        #endregion
    }
}
