using RPGTest.Models.Entity;
using RPGTest.Models;
using RPGTest.UI.Widgets;
using UnityEngine;

namespace RPGTest.UI.PartyMenu
{
    public class UI_MemberDetails_Widget : MonoBehaviour
    {
        [SerializeField] private UI_Equipment_Widget EquipmentWidget;
        [SerializeField] private UI_CharacterStats_Widget StatsWidget;

        private PresetSlot m_currentPreset;

        public void ChangePreset(PlayableCharacter character)
        {
            m_currentPreset = m_currentPreset == PresetSlot.First ? PresetSlot.Second : PresetSlot.First;
            Refresh(character);
        }

        public void Refresh(PlayableCharacter character)
        {
            if(character == null)
            {
                StatsWidget.Clear();
                EquipmentWidget.Clear();
            }
            else
            {
                StatsWidget.Refresh(character, m_currentPreset);
                EquipmentWidget.Refresh(character.EquipmentSlots, m_currentPreset);
            }
        }
    }
}
