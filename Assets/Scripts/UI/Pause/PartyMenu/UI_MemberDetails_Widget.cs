using RPGTest.Models.Entity;
using RPGTest.UI.Widgets;
using UnityEngine;

namespace RPGTest.UI.PartyMenu
{
    public class UI_MemberDetails_Widget : MonoBehaviour
    {
        [SerializeField] private UI_Equipment_Widget EquipmentWidget;
        [SerializeField] private UI_CharacterStats_Widget StatsWidget;
        
        public void Open(bool visible)
        {
            EquipmentWidget.gameObject.SetActive(visible);
        }

        public void SwapDisplay()
        {
            EquipmentWidget.gameObject.SetActive(!EquipmentWidget.gameObject.activeSelf);
            StatsWidget.gameObject.SetActive(!StatsWidget.gameObject.activeSelf);
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
