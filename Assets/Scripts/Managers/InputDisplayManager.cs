using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGTest.Managers
{
    public enum InputSupport
    {
        PC,
        PS,
        XBox,
    }

    public struct InputDisplay
    {
        public Texture2D[] Icons;

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
        [SerializeField]
        private InputDisplayConfiguration[] InputDisplays;

        public List<InputDisplay> GetInputDisplays(InputDevice device, Dictionary<string, string> inputs)
        {
            var support = InputDeviceToSupport(device);
            var displayConfiguration = InputDisplays.SingleOrDefault(i => i.Controller == support);

            var inputDisplays = new List<InputDisplay>();
            foreach(var display in displayConfiguration.Inputs)
            {
                if (inputs.TryGetValue(display.Action, out string description)) {
                    inputDisplays.Add( new InputDisplay{ Icons = display.Icons, Description = description });
                }
            }

            return inputDisplays;
        }

        private InputSupport InputDeviceToSupport(InputDevice device)
        {
            switch (device.name) {
                case "Mouse":
                case "Keyboard":
                    return InputSupport.PC;
                default:
                    throw new Exception("Unrecognized platform");
            }
        }
    }
}
