using RPGTest.Enums;
using RPGTest.Extensions;
using RPGTest.Helpers;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using System.Collections.Generic;

namespace RPGTest.UI.Common
{
    public class UI_View_EntityAttributes : UI_View_BaseEntityComponent
    {

        IEnumerable<UI_Control_StatPillAttribute> m_attributes => GetComponentsInChildren<UI_Control_StatPillAttribute>();

        public override void Initialize(Entity entity)
        {
            m_entity = entity;
            InitializeInternal(m_entity.GetAttributes());
        }

        public override void Initialize(PlayableCharacter character, PresetSlot preset)
        {
            m_character = character;
            m_preset = preset;

            InitializeInternal(m_character.GetAttributes(preset));
        }

        public override void Clear()
        {
            var components = GetComponentsInChildren<UI_Control_StatPillAttribute>();
            foreach (var component in components)
            {
                component.Clean();
            }
        }

        public override void Refresh()
        {
            InitializeInternal(m_entity.GetAttributes());
        }

        public override void Refresh(PresetSlot preset)
        {
            InitializeInternal(m_character.GetAttributes(preset));
        }

        public override void Preview(PresetSlot preset, EquipmentSlot slot, Equipment equipment)
        {
            if (!m_character.EquipmentComponent.GetEquipmentSlots(preset).TryCopy(out var tempEquipments))
            {
                return;
            }

            tempEquipments[slot] = equipment;

            PreviewInternal(m_character.GetAttributes(tempEquipments));
        }

        public override void Unpreview()
        {
            m_attributes.ForEach(a => a.Unpreview());
        }

        private void InitializeInternal(Dictionary<Attribute, float> attributes)
        {
            m_attributes.ForEach(a => a.Initialize((int)attributes[a.Attribute]));
        }

        private void PreviewInternal(Dictionary<Attribute, float> attributes)
        {
            m_attributes.ForEach(a => a.Preview((int)attributes[a.Attribute]));
        }
    }
}
