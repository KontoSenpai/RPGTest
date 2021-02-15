using RPGTest.Helpers;
using RPGTest.Inputs;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI
{
    public class UI_Menu : UI_Base
    {
        [SerializeField] private Button DefaultButton;
        [SerializeField] private GameObject DefaultWidget;

        [SerializeField] private Button MapButton;
        [SerializeField] private GameObject MapWidget;

        [SerializeField] private UI_PauseButtonAnimatorController CategoryButtonController;

        public Button[] MenuButtons;
        public GameObject[] MenuWidgets;

        public bool IsSubMenuSelected = false;

        //UI control
        private int m_currentNavigationIndex = 0;

        private Controls m_playerInput;
        public void Awake()
        {
            m_playerInput = new Controls();
            m_playerInput.UI.CycleMenus.performed += CycleMenus_performed;
            m_playerInput.UI.Cancel.performed += Cancel_performed;
        }

        public void OnEnable() => m_playerInput.Enable();
        public void OnDisable()
        {
            MenuWidgets.ForEach(x => x.GetComponent<UI_SubMenu>().Clear());
            m_playerInput.Disable();
        }

        #region InputSystemEvents
        private void CycleMenus_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var cycle = obj.ReadValue<float>();
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

        private void Cancel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
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

        public void Close()
        {

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
