using RPGTest.Helpers;
using RPGTest.Inputs;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGTest.UI
{
    public class UI_Menu : UI_Base
    {
        [SerializeField] private Button DefaultButton;
        [SerializeField] private GameObject DefaultWidget;

        [SerializeField] private Button MapButton;
        [SerializeField] private GameObject MapWidget;

        private Controls m_playerInput;

        [SerializeField] private UI_PauseButtonAnimatorController CategoryButtonController;

        [SerializeField] private Button[] MenuButtons;
        [SerializeField] private GameObject[] MenuWidgets;

        public bool IsSubMenuSelected = false;

        //UI control
        private int m_currentNavigationIndex = 0;

        public void Awake()
        {
            m_playerInput = new Controls();
        }

        public void OnEnable() => m_playerInput.Enable();
        public void OnDisable()
        {
            MenuWidgets.ForEach(x => x.GetComponent<UI_SubMenu>().Clear());
            m_playerInput.Disable();
        }

        #region InputSystemEvents
        public void CycleMenus(InputValue value)
        {
            var cycle = value.Get<float>();
            int newIndex = m_currentNavigationIndex;
            if(cycle > 0 && m_currentNavigationIndex < MenuButtons.Count() - 1)
            {
                newIndex++;
            }
            else if(cycle < 0 && m_currentNavigationIndex > 0)
            {

                newIndex--;
            }

            if(newIndex != m_currentNavigationIndex)
            {
                m_currentNavigationIndex = newIndex;
                SelectSubMenu(MenuWidgets[m_currentNavigationIndex]);
            }
        }

        public void Close()
        {
            if(!IsSubMenuSelected)
            {
                UIClosed(this, null);
            }
        }
        #endregion

        public void InitializeDefault()
        {
            Initialize(DefaultButton, DefaultWidget);
        }

        public void InitializeMap()
        {
            Initialize(MapButton, MapWidget);
        }

        private void Initialize(Button button, GameObject widget)
        {
            UIOpened(this, null);
            foreach (var menu in MenuWidgets)
            {
                menu.GetComponent<UI_SubMenu>().Initialize();
                menu.GetComponent<UI_SubMenu>().SubMenuOpened += OpenSubMenu;
                menu.GetComponent<UI_SubMenu>().SubMenuClosed += ExitSubMenu;
            }
            button.interactable = false;
            button.Select();
            SelectSubMenu(widget);
        }

        #region ButtonEvents      
        public void SelectSubMenu(GameObject go)
        {
            m_currentNavigationIndex = Array.IndexOf(MenuWidgets, go);
            Array.ForEach(MenuWidgets, w => w.SetActive(Array.IndexOf(MenuWidgets, w) == m_currentNavigationIndex));
            MenuButtons[m_currentNavigationIndex].Select();
            Array.ForEach(MenuButtons, b => b.interactable = !(Array.IndexOf(MenuButtons, b) == m_currentNavigationIndex));

            go.GetComponent<UI_SubMenu>().OpenMenu();

        }
        #endregion


        private void OpenSubMenu()
        {
            MenuButtons[m_currentNavigationIndex].interactable = false;
            IsSubMenuSelected = true;
        }

        private void ExitSubMenu()
        {
            MenuButtons[m_currentNavigationIndex].interactable = true;
            MenuButtons[m_currentNavigationIndex].Select();
            IsSubMenuSelected = false;
        }
    }
}
