using RPGTest.Inputs;
using RPGTest.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

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
            if (!ShouldUpdateInputDisplay)
            {
                return;
            }

            InputManager.SchemeChanged += OnScheme_Changed;
            m_playerInput.Enable();
            UpdateInputDisplay();
        }

        protected void DisableControls()
        {
            if (!ShouldUpdateInputDisplay)
            {
                return;
            }

            InputManager.SchemeChanged -= OnScheme_Changed;
            m_playerInput.Disable();
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
