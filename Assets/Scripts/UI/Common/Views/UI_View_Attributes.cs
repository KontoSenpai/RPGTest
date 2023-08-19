using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.UI.Widgets;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public class UI_View_Attributes : UI_View_BaseEntityComponent
    {
        [SerializeField] private UI_DualStatsWidget HpMpDualWidget;
        [SerializeField] private UI_StatWidget StaminaWidget;

        [SerializeField] private UI_DualStatsWidget AtkDefDualWidget;
        [SerializeField] private UI_DualStatsWidget MagResDualWidget;

        public void Clear()
        {
            HpMpDualWidget.Clean();
            StaminaWidget.Clean();
            AtkDefDualWidget.Clean();
            MagResDualWidget.Clean();
        }

        public override void Initialize(Entity entity)
        {
            m_entity = entity;
            RefreshInternal(m_entity.GetAttributes());
        }

        public override void Initialize(PlayableCharacter character, PresetSlot preset)
        {
            m_character = character;
            m_preset = preset;

            RefreshInternal(m_character.GetAttributes(preset));
        }

        public override void Refresh()
        {
            RefreshInternal(m_entity.GetAttributes());
        }

        public override void Refresh(PresetSlot preset)
        {
            throw new System.NotImplementedException();
        }

        private void RefreshInternal(Dictionary<Attribute, float> attributes)
        {
            KeyValuePair<string, int> firstPair = new KeyValuePair<string, int>(Attribute.MaxHP.GetDescription(), (int)attributes[Attribute.MaxHP]);
            KeyValuePair<string, int> secondPair = new KeyValuePair<string, int>(Attribute.MaxMP.GetDescription(), (int)attributes[Attribute.MaxMP]);
            HpMpDualWidget.Refresh(firstPair, secondPair);

            StaminaWidget.Refresh(Attribute.MaxStamina.GetDescription(), (int)attributes[Attribute.MaxStamina]);

            firstPair = new KeyValuePair<string, int>(Attribute.Attack.GetDescription(), (int)attributes[Attribute.TotalAttack]);
            secondPair = new KeyValuePair<string, int>(Attribute.Defense.GetDescription(), (int)attributes[Attribute.TotalDefense]);
            AtkDefDualWidget.Refresh(firstPair, secondPair);

            firstPair = new KeyValuePair<string, int>(Attribute.Magic.GetDescription(), (int)attributes[Attribute.TotalMagic]);
            secondPair = new KeyValuePair<string, int>(Attribute.Resistance.GetDescription(), (int)attributes[Attribute.TotalResistance]);
            MagResDualWidget.Refresh(firstPair, secondPair);
        }
    }
}
