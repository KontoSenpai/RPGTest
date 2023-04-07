using RPGTest.Inputs;
using RPGTest.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGTest.UI
{
    // Special EventArgs class to hold info about Shapes.
    public class HintEventArgs : EventArgs
    {
        public HintEventArgs(List<InputDisplay> inputDisplays)
        {
            InputDisplays = inputDisplays;
        }

        public List<InputDisplay> InputDisplays { get; }
    }

    public partial class UI_Pause_SubMenu : MonoBehaviour
    {
        public bool IsSubMenuSelected { get; set; }
        
        protected InputDisplayManager InputManager { get; set; }
        
        protected Controls m_playerInput { get; set; }

        protected InputDevice m_currentDevice { get; set; }

        public virtual void Awake()
        {
            m_playerInput = new Controls();
            InputManager = FindObjectOfType<InputDisplayManager>();
        }

        public virtual void OnEnable() => m_playerInput.Enable();
        public virtual void OnDisable() => m_playerInput.Disable();

        public virtual void Initialize(bool refreshAll = true) { }
        
        public virtual void Clear() { }

        public virtual void OpenMenu(InputDevice device)
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

        protected virtual Dictionary<string, string> GetActions()
        {
            throw new NotImplementedException();
        }

        protected void UpdateIconDisplay(InputDevice device, Dictionary<string, string> actions)
        {
            var inputDisplays = InputManager.GetInputDisplays(device, actions);
            InputHintsChanged.Invoke(this, new HintEventArgs(inputDisplays));
        }

        [HideInInspector]
        public event EventHandler<HintEventArgs> InputHintsChanged;
        [HideInInspector]
        public MenuChangedHandler SubMenuOpened { get; set; }
        [HideInInspector]
        public MenuChangedHandler SubMenuClosed { get; set; }
        [HideInInspector]
        public delegate void MenuChangedHandler();
    }
}
