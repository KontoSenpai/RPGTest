using System;
using RPGTest.Models;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    public class PresetSlotSelectedEventArgs : EventArgs
    {
        public PresetSlotSelectedEventArgs(PresetSlot presetSlot)
        {
            PresetSlot = presetSlot;
        }

        public PresetSlot PresetSlot { get; }
    }

    public class UI_PresetSlotSelector : MonoBehaviour
    {
        [SerializeField]
        private Button FirstPreset;
        [SerializeField]
        private Button SecondPreset;

        private PresetSlot m_currentPresetSlot;

        [HideInInspector]
        public event EventHandler<PresetSlotSelectedEventArgs> PresetSlotSelected;

        public void Initialize()
        {
            ChangePresetSlotInternal(PresetSlot.First);
        }

        public void ChangePreset()
        {
            ChangePresetSlotInternal(m_currentPresetSlot == PresetSlot.First ? PresetSlot.Second : PresetSlot.First);
        }

        public PresetSlot GetCurrentPreset()
        {
            return m_currentPresetSlot;
        }

        private void ChangePresetSlotInternal(PresetSlot presetSlot)
        {
            m_currentPresetSlot = presetSlot;

            FirstPreset.interactable = m_currentPresetSlot == PresetSlot.Second;
            SecondPreset.interactable = m_currentPresetSlot == PresetSlot.First;

            PresetSlotSelected.Invoke(this, new PresetSlotSelectedEventArgs(m_currentPresetSlot));
        }
    }
}
