using RPGTest.Models.Entity;
using RPGTest.UI.Widgets;
using UnityEngine;

namespace RPGTest.UI.PartyMenu
{
    public class UI_MemberDetails_Widget : MonoBehaviour
    {
        [SerializeField] private UI_Equipment_Widget EquipmentWidget;

        public void Open(bool visible)
        {
            EquipmentWidget.gameObject.SetActive(visible);
        }

        public void Refresh(PlayableCharacter character)
        {
            EquipmentWidget.Refresh(character.EquipmentSlots);
            /*
            foreach(var preset in Presets)
            {
                preset.EnduranceValue.text = character.BaseAttributes.MaxStamina.ToString();

                preset.AttackValue.text = character.GetAttribute("TotalStrength" + preset.BackendName).ToString();
                preset.DefenseValue.text = character.GetAttribute("TotalConstitution" + preset.BackendName).ToString();
                preset.MagicValue.text = character.GetAttribute("TotalMental" + preset.BackendName).ToString();
                preset.ResistanceValue.text = character.GetAttribute("TotalResilience" + preset.BackendName).ToString();
            }
            */
        }

    }
}
