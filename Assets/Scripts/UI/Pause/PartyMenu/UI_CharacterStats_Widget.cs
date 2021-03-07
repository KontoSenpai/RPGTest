using RPGTest.Models.Entity;
using UnityEngine;

namespace RPGTest.UI.PartyMenu
{
    public class UI_CharacterStats_Widget : MonoBehaviour
    {
        [SerializeField] private UI_CharacterPresetStats_Widget StatsPreset1;
        [SerializeField] private UI_CharacterPresetStats_Widget StatsPreset2;

        public void Refresh(PlayableCharacter playableCharacter)
        {
            if(playableCharacter == null)
            {
                StatsPreset1.Refresh(null);
                StatsPreset2.Refresh(null);
            }
            else
            {
                StatsPreset1.Refresh(playableCharacter.GetAttributes());
                StatsPreset2.Refresh(playableCharacter.GetAttributes());
            }
        }
    }
}
