using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using System.Collections.Generic;

namespace RPGTest.UI.Common
{
    public class UI_View_EntityAttributes : UI_View_BaseEntityComponent
    {
        public void Clear()
        {
            var components = GetComponentsInChildren<UI_Control_Attribute>();
            foreach(var component in components)
            {
                component.Clean();
            }
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

        public override void Preview(PresetSlot preset, Slot slot, Equipment equipment)
        {
            // throw new System.NotImplementedException();
        }

        public override void Unpreview()
        {
            // throw new System.NotImplementedException();
        }

        private void RefreshInternal(Dictionary<Attribute, float> attributes)
        {
            var components = GetComponentsInChildren<UI_Control_Attribute>();
            foreach (var component in components)
            {
                component.Clean();
                component.Refresh((int)attributes[component.Attribute]);
            }
        }
    }
}
