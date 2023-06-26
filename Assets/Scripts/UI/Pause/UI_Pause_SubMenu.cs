using RPGTest.UI.Common;
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

    public partial class UI_Pause_SubMenu : UI_Dialog
    {
        // Navigation helpers
        protected bool m_navigateStarted = false;
        protected float WaitTimeBetweenPerforms = 0.4f;
        protected float m_performTimeStamp;

        public virtual void Initialize(bool refreshAll = true)
        {
        }
        
        public virtual void Clear() { }

        public virtual void Open(Dictionary<string, object> parameters)
        {
            base.Open();
            SubMenuOpened();
        }

        public virtual void CloseMenu() 
        {
            SubMenuClosed();
        }

        protected virtual void OnCancel_performed(InputAction.CallbackContext obj)
        {
            CloseMenu();
        }

        // Open another menu from a sub menu action
        protected void ChangeMenu(int index, Dictionary<string, object> parameters)
        {
            MenuChanged.Invoke(this, new MenuChangeEventArgs(index, parameters));
        }

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
