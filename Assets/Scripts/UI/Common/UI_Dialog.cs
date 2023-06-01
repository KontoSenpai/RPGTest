using RPGTest.Inputs;
using RPGTest.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public abstract class UI_Dialog : MonoBehaviour
    {
        [SerializeField] protected GameObject ConfirmButton;
        [SerializeField] protected GameObject CancelButton;


        [HideInInspector]
        public EventHandler<EventArgs> DialogActionCancelled;

        [HideInInspector]
        public EventHandler<EventArgs> DialogActionConfirmed;

        protected Dictionary<string, string[]> m_inputActions = new Dictionary<string, string[]>();
        protected Controls m_playerInput;
        protected InputDisplayManager InputManager => FindObjectOfType<InputDisplayManager>();

        public virtual void Awake()
        {
            m_playerInput = new Controls();
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
            gameObject.SetActive(true);
            EnableControls();
        }

        /// <summary>
        /// Close the dialog and disable user inputs
        /// </summary>
        public virtual void Close()
        {
            DisableControls();
            gameObject.SetActive(false);
        }

        protected void EnableControls()
        {
            InputManager.SchemeChanged += OnScheme_Changed;
            m_playerInput.Enable();
            UpdateInputDisplay();
        }

        protected void DisableControls()
        {
            InputManager.SchemeChanged -= OnScheme_Changed;
            m_playerInput.Disable();
        }

        protected virtual void UpdateInputActions()
        {
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
