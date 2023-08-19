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

    public abstract class UI_Pause_SubMenu : UI_Dialog
    {
        // Navigation helpers
        protected bool m_navigateStarted = false;
        protected float WaitTimeBetweenPerforms = 0.4f;
        protected float m_performTimeStamp;
        
        public virtual void Clear() { }

        /// <summary>
        /// Execute once when the PauseMenu is opened
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Execute each time the subMenu is getting opened
        /// </summary>
        /// <param name="parameters"></param>
        public virtual void OpenSubMenu(Dictionary<string, object> parameters)
        {
            base.Open();
            SubMenuOpened();
        }

        /// <summary>
        /// Execute each time the subMenu is getting closed
        /// </summary>
        public virtual void CloseSubMenu()
        {
            base.Close();
        }

        /// <summary>
        /// Executed when a Cancel action has been used on the "root" level of a sub menu
        /// Will clean and close the SubMenu
        /// </summary>
        public virtual void ExitPause() 
        {
            SubMenuClosed();
        }

        protected virtual void OnCancel_performed(InputAction.CallbackContext obj)
        {
            ExitPause();
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
