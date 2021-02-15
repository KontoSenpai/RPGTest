using RPGTest.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.PartyMenu
{
    public class UI_SubMenu_Party : UI_SubMenu
    {
        public GameObject PartyList;
        public GameObject PartyItemInstantiate;
        public UI_Stats_Widget StatsWidget;
        public int MaxMembers = 8;
        PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        //Items control
        private List<GameObject> m_allMembers = new List<GameObject>();
        private int m_indexToSwap = 0;
        private int m_currentNavigationIndex = 0;
        private bool m_swapInProgress = false;

        
        public override void Awake()
        {
            base.Awake();
            m_playerInput.UI.Navigate.performed += Navigate_performed;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            m_playerInput.UI.Cancel.performed += Cancel_performed;
        }

        public override void OnDisable() => m_playerInput.Disable();


        protected override void Cancel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(m_swapInProgress)
            {
                m_allMembers[m_indexToSwap].GetComponent<UI_Member_Widget>().Select();
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
                if (movement.y > 0 && m_currentNavigationIndex > 0)
                {
                    m_currentNavigationIndex -= 1;
                }
                else if (movement.y < 0 && m_currentNavigationIndex < m_allMembers.Count - 1)
                {
                    m_currentNavigationIndex += 1;
                }
                RefreshDetailsPanel();
            }
        }

        public override void OpenMenu()
        {
            base.OpenMenu();
            m_playerInput.UI.Cancel.performed += Cancel_performed;
            if (m_allMembers.Count > 0)
            {
                m_allMembers[0].GetComponent<Button>().Select();
            }
            StatsWidget.SetVisible(true);
            RefreshDetailsPanel();
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
            var elementHeight = PartyList.GetComponent<RectTransform>().rect.height / MaxMembers; 
            foreach (var member in m_partyManager.GetAllPartyMembers())
            {
                var uiMember = Instantiate(PartyItemInstantiate);
                uiMember.transform.SetParent(PartyList.transform);
                uiMember.name = member.Id;
                uiMember.transform.localScale = new Vector3(1, 1, 1);

                var widgetScript = uiMember.GetComponent<UI_Member_Widget>();
                widgetScript.Initialize(member);
                widgetScript.SetSecondarySelect(() => widgetScript.ToggleCover());
                widgetScript.MemberSelected += SwapMembers;
                m_allMembers.Add(uiMember);
            }

            //Expliciting navigation
            foreach(var member in m_allMembers)
            {
                var index = m_allMembers.IndexOf(member);
                Navigation newNavigation = new Navigation();
                newNavigation.mode = Navigation.Mode.Explicit;

                if(index > 0)
                {
                    newNavigation.selectOnUp = m_allMembers[index - 1].GetComponent<Button>();
                }
                if(index < m_allMembers.Count -1)
                {
                    newNavigation.selectOnDown = m_allMembers[index + 1].GetComponent<Button>();
                }
                member.GetComponent<Button>().navigation = newNavigation;
            }
        }

        public override void Clear()
        {
            m_allMembers.ForEach(x => Destroy(x));
            m_allMembers.Clear();
        }

        public void RefreshDetailsPanel()
        {
            StatsWidget.Refresh(m_partyManager.GetAllPartyMembers()[m_currentNavigationIndex]);
        }

        /// <summary>
        /// Trigged by an Event sent with a Button press on a party member.
        /// </summary>
        /// <param name="selectedItem">Selected party member</param>
        public void SwapMembers(GameObject selectedItem)
        {
            if(!m_swapInProgress)
            {
                m_indexToSwap = m_allMembers.IndexOf(selectedItem);
                m_swapInProgress = true;
            }
            else
            {
                var indexToSwapWith = m_allMembers.IndexOf(selectedItem);
                if (indexToSwapWith != m_indexToSwap)
                {
                    m_partyManager.SwapMemberPosition(m_indexToSwap, indexToSwapWith);
                    Refresh();
                    m_allMembers[m_currentNavigationIndex].GetComponent<Button>().Select();
                    RefreshDetailsPanel();
                }
                m_swapInProgress = false;
            }
        }

        private void Refresh()
        {
            foreach(var member in m_allMembers)
            {
                member.GetComponent<UI_Member_Widget>().MemberSelected -= SwapMembers;
            }
            Clear();
            Initialize(false);
        }
    }
}
