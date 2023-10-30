using RPGTest.Inputs;
using RPGTest.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGTest.UI.Common
{
    public class UI_View : MonoBehaviour
    {
        public bool ShouldUpdateInputDisplay = true;

        protected Dictionary<string, string[]> m_inputActions = new Dictionary<string, string[]>();
        protected Controls m_playerInput;
        protected InputDisplayManager InputManager => FindObjectOfType<InputDisplayManager>();

        public virtual void Awake()
        {
            m_playerInput = new Controls();
            m_playerInput.Disable();
        }

        public virtual void OnEnable()
        {
            m_playerInput.Enable();
        }

        public virtual void OnDisable()
        {
            m_playerInput.Disable();
        }

        /// <summary>
        /// Sets the focus on the view and enable input actions
        /// </summary>
        public virtual void Select()
        {
            EnableControls();
        }

        /// <summary>
        /// Remove focus from the view and disable input actions
        /// </summary>
        public virtual void Deselect()
        {
            DisableControls();
        }

        /// <summary>
        /// Open the dialog and enable user inputs
        /// </summary>
        public virtual void Open()
        {
            if (gameObject.activeSelf)
                return;

            gameObject.SetActive(true);
            EnableControls();
        }

        /// <summary>
        /// Close the dialog and disable user inputs
        /// </summary>
        public virtual void Close()
        {
            if (gameObject.activeSelf == false) return;

            DisableControls();
            gameObject.SetActive(false);
        }

        protected void EnableControls()
        {
            InputManager.SchemeChanged += OnScheme_Changed;
            m_playerInput.Enable();

            if (!ShouldUpdateInputDisplay)
            {
                return;
            }
            UpdateInputDisplay();
        }

        protected void DisableControls()
        {
            if (InputManager)
            {
                InputManager.SchemeChanged -= OnScheme_Changed;
            }

            if (m_playerInput != null)
            {
                m_playerInput.Disable();
            }
        }

        /// <summary>
        /// Get all input actions permitted by current view
        /// </summary>
        /// <param name="playerInput">input controls, to use in case the view input gets requested before it gets awakened</param>
        /// <returns>List of inputs defined for the view</returns>
        public virtual Dictionary<string, string[]> GetInputDisplay(Controls playerInput = null)
        {
            return new Dictionary<string, string[]>();
        }

        protected virtual void UpdateInputActions()
        {
            if (!ShouldUpdateInputDisplay)
            {
                return;
            }

            UpdateInputDisplay();
        }

        protected virtual void UpdateInputDisplay()
        {
            FindObjectOfType<UI_Controls_Display>(false).Refresh(InputManager.GetInputDisplays(m_inputActions));
        }

        protected void OnScheme_Changed(object sender, EventArgs e)
        {
            UpdateInputDisplay();
        }
    }
}
