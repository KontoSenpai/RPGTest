using RPGTest.Inputs;
using RPGTest.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGTest.UI
{
    public class HintEventArgs : EventArgs
    {
        public HintEventArgs(List<InputDisplay> inputDisplays)
        {
            InputDisplays = inputDisplays;
        }

        public List<InputDisplay> InputDisplays { get; }
    }

    public class MenuChangeEventArgs: EventArgs
    {
        public MenuChangeEventArgs(int menuIndex, Dictionary<string, object> parameters)
        {
            MenuIndex = menuIndex;
            Parameters = parameters;
        }

        public int MenuIndex{ get; }
        public Dictionary<string, object> Parameters { get; }
    }

    public partial class UI_Pause_SubMenu : MonoBehaviour
    {
        public bool IsSubMenuSelected { get; set; }
        
        protected InputDisplayManager InputManager { get; set; }
        
        protected Controls m_playerInput { get; set; }

        public virtual void Awake()
        {
            m_playerInput = new Controls();
            InputManager = FindObjectOfType<InputDisplayManager>();
        }

        public virtual void OnEnable()
        {
            m_playerInput.Enable();
        }

        public virtual void OnDisable()
        {
            m_playerInput.Disable();
            FindObjectOfType<InputDisplayManager>().DeviceChanged -= onDevice_Changed;
        }

        public virtual void Initialize(bool refreshAll = true)
        {
        }
        
        public virtual void Clear() { }

        public virtual void OpenMenu(Dictionary<string, object> parameters)
        {
            IsSubMenuSelected = true;
            SubMenuOpened();
        }

        public virtual void CloseMenu() 
        {
            IsSubMenuSelected = false;
            SubMenuClosed();
        }

        protected virtual void Cancel_performed(InputAction.CallbackContext obj)
        {
            if(IsSubMenuSelected)
            {
                CloseMenu();
            }
        }

        protected virtual Dictionary<string, string[]> GetInputActionDescriptions()
        {
            throw new NotImplementedException();
        }

        protected virtual void onDevice_Changed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        // Update the input Hints on current page
        protected void UpdateIconDisplay(Dictionary<string, string[]> actions)
        {
            var inputDisplays = InputManager.GetInputDisplays(actions);
            InputHintsChanged.Invoke(this, new HintEventArgs(inputDisplays));
        }

        // Open another menu from a sub menu action
        protected void ChangeMenu(int index, Dictionary<string, object> parameters)
        {
            MenuChanged.Invoke(this, new MenuChangeEventArgs(index, parameters));
        }

        [HideInInspector]
        public event EventHandler<HintEventArgs> InputHintsChanged;
        [HideInInspector]
        public event EventHandler<MenuChangeEventArgs> MenuChanged;
        [HideInInspector]
        public MenuChangedHandler SubMenuOpened { get; set; }
        [HideInInspector]
        public MenuChangedHandler SubMenuClosed { get; set; }
        [HideInInspector]
        public delegate void MenuChangedHandler();
    }
}
