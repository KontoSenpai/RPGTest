using RPGTest.Helpers;
using RPGTest.Inputs;
using RPGTest.Managers;
using System;
using System.Collections.Generic;
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
        [SerializeField] private GameObject MenuFooter;

        public bool IsSubMenuSelected = false;

        //UI control
        private int m_currentNavigationIndex = 0;
        private int m_pendingNavigationIndex = 0;

        public void Awake()
        {
            m_playerInput = new Controls();
            m_playerInput.UI.CycleMenus.performed += CycleMenus_performed;
            m_playerInput.UI.CloseMenu.performed += ctx =>
            {
                DefaultButton.Select();
                UIClosed(this, null);
            };
        }

        public void OnEnable()
        {
            m_playerInput.Enable();
        }

        public void OnDisable()
        {
            MenuWidgets.ForEach(x =>
            {
                x.GetComponent<UI_Pause_SubMenu>().Clear();
                x.GetComponent<UI_Pause_SubMenu>().MenuChanged += OnMenu_changed;
            });
            m_playerInput.Disable();
        }

        #region EventHandlers
        private void OnMenu_changed(object sender, MenuChangeEventArgs e)
        {
            SelectSubMenu(e.MenuIndex, e.Parameters);
        }
        #endregion

        #region InputSystemEvents
        private void CycleMenus_performed(InputAction.CallbackContext ctx)
        {
            var cycle = ctx.ReadValue<float>();
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
                SelectSubMenu(newIndex);
            }
        }
        private void Navigate_performed(InputAction.CallbackContext ctx)
        {
            if(IsSubMenuSelected)
            {
                return;
            }

            var movement = ctx.ReadValue<Vector2>();
            int newIndex = m_pendingNavigationIndex;
            if (movement.x > 0 && m_pendingNavigationIndex < MenuButtons.Count() - 1)
            {
                newIndex++;
            }
            else if (movement.x < 0 && m_pendingNavigationIndex > 0)
            {
                newIndex--;
            }

            MenuButtons[newIndex].Select();

            if (newIndex != m_pendingNavigationIndex)
            {
                m_pendingNavigationIndex = newIndex;
            }
        }

        public void Submit_Performed(InputAction.CallbackContext ctx)
        {
            SelectSubMenu(m_pendingNavigationIndex);
        }

        public void Cancel_Performed(InputAction.CallbackContext ctx)
        {
            if(!IsSubMenuSelected)
            {
                UIClosed(this, null);
            }
        }

        public void MouseMoved_Performed(InputAction.CallbackContext ctx)
        {

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
                if(menu.GetComponent<UI_Pause_SubMenu>() != null)
                {
                    menu.GetComponent<UI_Pause_SubMenu>().Initialize();
                    menu.GetComponent<UI_Pause_SubMenu>().SubMenuOpened += OpenSubMenu;
                    menu.GetComponent<UI_Pause_SubMenu>().SubMenuClosed += ExitSubMenu;
                    menu.GetComponent<UI_Pause_SubMenu>().MenuChanged += OnMenu_changed;
                }
            }
            button.Select();
            button.interactable = false;
            SelectSubMenu(Array.IndexOf(MenuWidgets, widget));
        }

        #region ButtonEvents      
        public void SelectSubMenu(int menuIndex, Dictionary<string, object> parameters = null)
        {
            m_currentNavigationIndex = menuIndex;
            Array.ForEach(MenuWidgets, w => w.SetActive(Array.IndexOf(MenuWidgets, w) == m_currentNavigationIndex));
            MenuButtons[m_currentNavigationIndex].Select();
            Array.ForEach(MenuButtons, b => b.interactable = !(Array.IndexOf(MenuButtons, b) == m_currentNavigationIndex));

            MenuWidgets[m_currentNavigationIndex].GetComponent<UI_Pause_SubMenu>().OpenMenu(parameters ?? new Dictionary<string, object>());
        }
        #endregion

        private void OpenSubMenu()
        {
            m_playerInput.UI.Navigate.performed -= Navigate_performed;
            m_playerInput.UI.Submit.performed -= Submit_Performed;
            m_playerInput.UI.Cancel.performed -= Cancel_Performed;

            MenuButtons[m_currentNavigationIndex].interactable = false;
            IsSubMenuSelected = true;
        }

        private void ExitSubMenu()
        {
            m_playerInput.UI.Navigate.performed += Navigate_performed;
            m_playerInput.UI.Submit.performed += Submit_Performed;
            m_playerInput.UI.Cancel.performed += Cancel_Performed;
            
            MenuButtons[m_currentNavigationIndex].interactable = true;
            MenuButtons[m_currentNavigationIndex].Select();
            IsSubMenuSelected = false;

            m_pendingNavigationIndex = m_currentNavigationIndex;
        }
    }
}
