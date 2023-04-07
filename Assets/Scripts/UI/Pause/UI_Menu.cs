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
        [SerializeField] private GameObject MenuFooter;

        public bool IsSubMenuSelected = false;

        //UI control
        private int m_currentNavigationIndex = 0;
        private int m_pendingNavigationIndex = 0;

        public void Awake()
        {
            m_playerInput = new Controls();
            m_playerInput.UI.CycleMenus.performed += CycleMenus_performed;
        }

        public void OnEnable() => m_playerInput.Enable();
        public void OnDisable()
        {
            MenuWidgets.ForEach(x => x.GetComponent<UI_Pause_SubMenu>().Clear());
            m_playerInput.Disable();
        }

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
                m_currentNavigationIndex = newIndex;
                SelectSubMenu(ctx.control.device, MenuWidgets[m_currentNavigationIndex]);
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
            SelectSubMenu(ctx.control.device, MenuWidgets[m_pendingNavigationIndex]);
        }

        public void Cancel_Performed(InputAction.CallbackContext ctx)
        {
            if(!IsSubMenuSelected)
            {
                UIClosed(this, null);
            }
        }
        #endregion

        public void InitializeDefault(InputDevice device)
        {
            Initialize(device, DefaultButton, DefaultWidget);
        }

        public void InitializeMap(InputDevice device)
        {
            Initialize(device, MapButton, MapWidget);
        }

        private void Initialize(InputDevice device, Button button, GameObject widget)
        {
            UIOpened(this, null);
            foreach (var menu in MenuWidgets)
            {
                if(menu.GetComponent<UI_Pause_SubMenu>() != null)
                {
                    menu.GetComponent<UI_Pause_SubMenu>().Initialize();
                    menu.GetComponent<UI_Pause_SubMenu>().SubMenuOpened += OpenSubMenu;
                    menu.GetComponent<UI_Pause_SubMenu>().SubMenuClosed += ExitSubMenu;
                    menu.GetComponent<UI_Pause_SubMenu>().InputHintsChanged += UpdateHintsFooter;
                }
            }
            button.Select();
            button.interactable = false;
            SelectSubMenu(device, widget);
        }

        private void UpdateHintsFooter(object sender, HintEventArgs e)
        {
            MenuFooter.GetComponent<UI_Footer_Hints>().Refresh(e.InputDisplays);
        }

        #region ButtonEvents      
        public void SelectSubMenu(InputDevice device, GameObject go)
        {
            m_currentNavigationIndex = Array.IndexOf(MenuWidgets, go);
            Array.ForEach(MenuWidgets, w => w.SetActive(Array.IndexOf(MenuWidgets, w) == m_currentNavigationIndex));
            MenuButtons[m_currentNavigationIndex].Select();
            Array.ForEach(MenuButtons, b => b.interactable = !(Array.IndexOf(MenuButtons, b) == m_currentNavigationIndex));

            go.GetComponent<UI_Pause_SubMenu>().OpenMenu(device);
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
