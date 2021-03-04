using RPGTest.Inputs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGTest.UI
{
    public partial class UI_Pause_SubMenu : MonoBehaviour
    {
        public bool IsSubMenuSelected { get; set; }
        protected Controls m_playerInput;
        
        public virtual void Awake()
        {
            m_playerInput = new Controls();
            m_playerInput.UI.Cancel.performed += Cancel_performed;
        }

        public virtual void OnEnable() => m_playerInput.Enable();
        public virtual void OnDisable() => m_playerInput.Disable();

        public virtual void Initialize(bool refreshAll = true) { }
        
        public virtual void Clear() { }

        public virtual void OpenMenu()
        {
            IsSubMenuSelected = true;
            SubMenuOpened();
        }

        public virtual void CloseMenu() 
        {
            IsSubMenuSelected = false;
            SubMenuClosed();
        }

        protected virtual void Cancel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(IsSubMenuSelected)
            {
                CloseMenu();
            }
        }


        [HideInInspector]
        public MenuChangedHandler SubMenuOpened { get; set; }
        [HideInInspector]
        public MenuChangedHandler SubMenuClosed { get; set; }
        [HideInInspector]
        public delegate void MenuChangedHandler();
    }
}
