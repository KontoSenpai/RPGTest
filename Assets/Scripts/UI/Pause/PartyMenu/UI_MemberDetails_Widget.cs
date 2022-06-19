using RPGTest.Models.Entity;
using RPGTest.UI.Widgets;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.UI.PartyMenu
{
    public class UI_MemberDetails_Widget : MonoBehaviour
    {
        [SerializeField] private UI_Equipment_Widget EquipmentWidget;
        [SerializeField] private UI_CharacterStats_Widget StatsWidget;

        private List<GameObject> m_widgets;
        private int index = 0;

        public void Open(bool visible)
        {
            index = 0;
            m_widgets = new List<GameObject> {
                EquipmentWidget.gameObject,
                StatsWidget.gameObject
            };

            m_widgets[0].SetActive(visible);
        }

        public void ChangeDisplay(bool cycleRight)
        {
            m_widgets[index].SetActive(false);
            if (cycleRight && index < m_widgets.Count - 1)
            {
                index++;
            }
            else if (cycleRight && index >= m_widgets.Count -1) // Cycle over
            {
                index = 0;
            }
            else if(!cycleRight && index > 0)
            {
                index--;
            }
            else if(!cycleRight && index <= 0)
            {
                index = m_widgets.Count - 1;
            }
            m_widgets[index].SetActive(true);
        }

        public void Refresh(PlayableCharacter character)
        {
            if(character == null)
            {
                EquipmentWidget.Refresh(null);
            }
            else
            {
                EquipmentWidget.Refresh(character.EquipmentSlots);
            }

            StatsWidget.Refresh(character);
        }

    }
}
