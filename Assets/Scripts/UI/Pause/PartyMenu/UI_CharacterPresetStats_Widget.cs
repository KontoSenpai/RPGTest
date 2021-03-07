using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.UI.Widgets;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.UI.PartyMenu
{
    public class UI_CharacterPresetStats_Widget : MonoBehaviour
    {
        [SerializeField] private UI_DualStatsWidget HpMpDualWidget;
        [SerializeField] private UI_StatWidget StaminaWidget;

        [SerializeField] private UI_DualStatsWidget AtkDefDualWidget;
        [SerializeField] private UI_DualStatsWidget MagResDualWidget;
        [SerializeField] private UI_DualStatsWidget BlcEvaDualWidget;
        [SerializeField] private UI_DualStatsWidget AccSpdDualWidget;

        public void Refresh(Dictionary<Attribute, float> attributes)
        {
            if(attributes == null)
            {
                HpMpDualWidget.Clean();
                StaminaWidget.Clean();
                AtkDefDualWidget.Clean();
                MagResDualWidget.Clean();
                BlcEvaDualWidget.Clean();
                AccSpdDualWidget.Clean();
                return;
            }

            KeyValuePair<string, int> firstPair = new KeyValuePair<string, int>(Attribute.MaxHP.GetDescription(), (int)attributes[Attribute.MaxHP]);
            KeyValuePair<string, int> secondPair = new KeyValuePair<string, int>(Attribute.MaxMP.GetDescription(), (int)attributes[Attribute.MaxMP]);
            HpMpDualWidget.Refresh(firstPair, secondPair);

            StaminaWidget.Refresh(Attribute.MaxStamina.GetDescription(), (int)attributes[Attribute.MaxStamina]);

            firstPair = new KeyValuePair<string, int>(Attribute.Attack.GetDescription(), (int)attributes[Attribute.TotalAttackP1]);
            secondPair = new KeyValuePair<string, int>(Attribute.Defense.GetDescription(), (int)attributes[Attribute.TotalDefenseP2]);
            AtkDefDualWidget.Refresh(firstPair,secondPair);
        }

    }
}
