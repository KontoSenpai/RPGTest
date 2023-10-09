using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RPGTest.Inputs;
using UnityEngine.InputSystem.Users;

namespace RPGTest.Managers
{
    public enum InputSupport
    {
        PC,
        PS,
        XBox,
    }

    public class InputDisplay
    {
        public List<Texture2D[]> Icons = new List<Texture2D[]>();

        public string Description;
    }

    [Serializable]
    public struct ActionDisplayConfiguration
    {
        public string Action;

        public Texture2D[] Icons;
    }

    [Serializable]
    public struct InputDisplayConfiguration
    {
        public InputSupport Controller;
        
        public ActionDisplayConfiguration[] Inputs;
    }

    public class InputDisplayManager : MonoBehaviour
    {
        [HideInInspector]
        public event EventHandler<EventArgs> SchemeChanged;

        [SerializeField]
        private InputDisplayConfiguration[] InputDisplays;

        private Controls m_playerInput;

        private InputControlScheme? m_currentControlScheme;

        private void Awake()
        {
            m_playerInput = new Controls();
            InputUser.onChange += onChange;
        }

        public virtual void OnEnable() => m_playerInput.Enable();
        public virtual void OnDisable() => m_playerInput.Disable();

        private void onChange(InputUser user, InputUserChange inputUserChange, InputDevice device)
        {
            if (user.controlScheme.HasValue && inputUserChange == InputUserChange.ControlSchemeChanged)
            {
                if (m_currentControlScheme == user.controlScheme) return;
                m_currentControlScheme = user.controlScheme;

                if (SchemeChanged != null && SchemeChanged.GetInvocationList().Length > 0)
                {
                    SchemeChanged.Invoke(this, new EventArgs());
                }
            }
        }

        public List<InputDisplay> GetInputDisplays(Dictionary<string, string[]> actions)
        {
            var support = InputSchemeToSupport();
            var displayConfiguration = InputDisplays.SingleOrDefault(i => i.Controller == support);

            var inputDisplays = new List<InputDisplay>();
            foreach(var action in actions)
            {
                var inputDisplay = new InputDisplay { Description = action.Key };
                foreach(var a in action.Value)
                {
                    if(displayConfiguration.Inputs.Count(i => i.Action == a) == 1) {
                        var input = displayConfiguration.Inputs.SingleOrDefault(x => x.Action == a);
                        inputDisplay.Icons.Add(input.Icons);
                    }
                }
                if (inputDisplay.Icons.Count > 0)
                {
                    inputDisplays.Add(inputDisplay);
                }
            }
            return inputDisplays;
        }

        private InputSupport InputSchemeToSupport()
        {
            if (!m_currentControlScheme.HasValue)
            {
                m_currentControlScheme = InputUser.all[0].controlScheme;
            }
            switch (m_currentControlScheme.Value.name) {
                case "PC":
                    return InputSupport.PC;
                case "PS":
                    return InputSupport.PS;
                default:
                    throw new Exception("Unrecognized platform");
            }
        }
    }
}
