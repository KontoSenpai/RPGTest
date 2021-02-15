using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.UI.Widgets;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.PartyMenu
{
    public class UI_Stats_Widget : MonoBehaviour
    {
        public UI_PresetStats_Widget[] Presets;

        public void SetVisible(bool visible)
        {
            foreach(var preset in Presets)
            {
                preset.gameObject.SetActive(visible);
            }
        }

        public void Refresh(PlayableCharacter character)
        {
            foreach(var preset in Presets)
            {
                preset.EnduranceValue.text = character.BaseAttributes.MaxStamina.ToString();

                preset.AttackValue.text = character.GetAttribute("TotalStrength" + preset.BackendName).ToString();
                preset.DefenseValue.text = character.GetAttribute("TotalConstitution" + preset.BackendName).ToString();
                preset.MagicValue.text = character.GetAttribute("TotalMental" + preset.BackendName).ToString();
                preset.ResistanceValue.text = character.GetAttribute("TotalResilience" + preset.BackendName).ToString();
            }
        }

    }
}
