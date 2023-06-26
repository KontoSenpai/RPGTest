using RPGTest.Helpers;
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
        [SerializeField] private Button MapButton;
        [SerializeField] private UI_Pause_SubMenu MapSubMenu;

        [SerializeField] private UI_PauseButtonAnimatorController CategoryButtonController;

        [SerializeField] private Button[] MenuButtons;
        [SerializeField] private UI_Pause_SubMenu[] SubMenus;

        //UI control
        private int m_currentNavigationIndex = -1;

        public override void Awake()
        {
            base.Awake();
            m_playerInput.UI.CycleMenus.performed += CycleMenus_performed;
            foreach (var menu in SubMenus)
            {
                if (menu != null)
                {
                    menu.Initialize();
                    menu.SubMenuOpened += OpenSubMenu;
                    menu.SubMenuClosed += ExitSubMenu;
                    menu.MenuChanged += OnMenu_changed;
                }
            }
        }

        public void OnEnable()
        {
            m_playerInput.Enable();
        }

        public void OnDisable()
        {
            SubMenus.ForEach(x =>
            {
                x.Clear();
            });
            m_playerInput.Disable();
        }

        #region EventHandlers
        /// <summary>
        /// Handle event received on button category click.
        /// </summary>
        /// <param name="menuIndex">Index of the menu to open</param>
        public void OnMenu_Selected(UI_Pause_SubMenu subMenu)
        {
            SelectSubMenu(Array.IndexOf(SubMenus, subMenu));
        }

        /// <summary>
        /// Handle event sent by sub menus, that send a sub menu change
        /// ie. Party Menu asking a redirection to Equipment/Skills
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        #endregion

        public void OpenDefault()
        {
            Open(0);
        }

        public void OpenMap()
        {
            Open(Array.IndexOf(SubMenus, MapSubMenu));
        }

        private void Open(int menuIndex)
        {
            UIOpened(this, null);
            MenuButtons[menuIndex].Select();
            MenuButtons[menuIndex].interactable = false;
            SelectSubMenu(menuIndex);
        }

        private void SelectSubMenu(int menuIndex, Dictionary<string, object> parameters = null)
        {
            if (m_currentNavigationIndex != -1)
            {
                MenuButtons[m_currentNavigationIndex].interactable = true;
                SubMenus[m_currentNavigationIndex].Close();
            }

            m_currentNavigationIndex = menuIndex;
            MenuButtons[m_currentNavigationIndex].interactable = false;
            SubMenus[m_currentNavigationIndex].Open(parameters ?? new Dictionary<string, object>());
        }

        private void OpenSubMenu()
        {
        }

        private void ExitSubMenu()
        {
            UIClosed(this, null);
        }
    }
}
