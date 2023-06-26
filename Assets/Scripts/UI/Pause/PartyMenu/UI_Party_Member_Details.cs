using UnityEngine;
using RPGTest.Models.Entity;
using RPGTest.Models;
using RPGTest.UI.Common;

namespace RPGTest.UI.PartyMenu
{
    public class UI_Party_Member_Details : MonoBehaviour
    {
        [SerializeField] private UI_Equipment_View EquipmentWidget;
        [SerializeField] private UI_Party_Member_Stats StatsWidget;

        private PresetSlot m_currentPreset;

        public void ChangePreset(PlayableCharacter character)
        {
            m_currentPreset = m_currentPreset == PresetSlot.First ? PresetSlot.Second : PresetSlot.First;
            Refresh(character);
        }

        public void Refresh(PlayableCharacter character)
        {
            EnableWidgets(character != null);
            if (character == null)
                return;

            StatsWidget.Refresh(character, m_currentPreset);
            EquipmentWidget.Refresh(character.EquipmentSlots, m_currentPreset);
        }

        private void EnableWidgets(bool value)
        {
            StatsWidget.enabled = value;
            EquipmentWidget.enabled = value;
        }
    }
}
