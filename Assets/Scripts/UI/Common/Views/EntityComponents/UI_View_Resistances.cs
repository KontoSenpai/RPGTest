using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public enum UI_ResistanceType
    {
        Elements,
        StatusEffects,
    }

    // Class to display
    public class UI_View_Resistances : UI_View_BaseEntityComponent
    {
        [SerializeField] private UI_ResistanceType ResistanceType;

        private List<UI_Control_Resistance> m_resistances
        {
            get { return this.GetComponentsInChildren<UI_Control_Resistance>().ToList(); }
        }

        public override void Initialize(Entity entity)
        {
            m_entity = entity;

            switch (ResistanceType)
            {
                case UI_ResistanceType.Elements:
                    InitializeInternal(m_entity.GetElementalResistances());
                    break;
                case UI_ResistanceType.StatusEffects:
                    InitializeInternal(m_entity.GetStatusEffectResistances());
                    break;
                default:
                    throw new System.Exception("Unsupported resistance type");
            }
        }

        public override void Initialize(PlayableCharacter character, PresetSlot preset)
        {
            m_character = character;
            m_preset = preset;

            switch (ResistanceType)
            {
                case UI_ResistanceType.Elements:
                    InitializeInternal(m_character.GetElementalResistances(m_preset));
                    break;
                case UI_ResistanceType.StatusEffects:
                    InitializeInternal(m_character.GetStatusEffectResistances(m_preset));
                    break;
                default:
                    throw new System.Exception("Unsupported resistance type");
            }
        }

        public override void Refresh()
        {
            throw new System.NotImplementedException();
        }

        public override void Refresh(PresetSlot slot)
        {
            throw new System.NotImplementedException();
        }

        public override void Preview(PresetSlot preset, Slot slot, Equipment equipment)
        {
            if (!m_character.EquipmentSlots.GetEquipmentPreset(preset).TryCopy(out var tempEquipments))
            {
                return;
            }

            tempEquipments[slot] = equipment;

            switch (ResistanceType)
            {
                case UI_ResistanceType.Elements:
                    PreviewInternal(m_character.GetElementalResistances(tempEquipments));
                    break;
                case UI_ResistanceType.StatusEffects:
                    PreviewInternal(m_character.GetStatusEffectResistances(tempEquipments));
                    break;
                default:
                    throw new System.Exception("Unsupported resistance type");
            }
        }

        public override void Unpreview()
        {
            m_resistances.ForEach(r => r.Unpreview());
        }

        private void InitializeInternal(Dictionary<Element, float> elementalResistances)
        {
            foreach (var elementalResistance in elementalResistances)
            {
                var resistance = m_resistances.SingleOrDefault((r) => r.Element == elementalResistance.Key);
                if (resistance != null)
                {
                    resistance.Initialize(elementalResistance.Value);
                }
            }
        }

        private void InitializeInternal(Dictionary<StatusEffect, float> statusEffectResistances)
        {
            foreach (var statusEffectResistance in statusEffectResistances)
            {
                var resistance = m_resistances.SingleOrDefault((r) => r.Status == statusEffectResistance.Key);
                if (resistance != null)
                {
                    resistance.Initialize(statusEffectResistance.Value);
                }
            }
        }

        private void PreviewInternal(Dictionary<Element, float> elementalResistances)
        {
            foreach (var elementalResistance in elementalResistances)
            {
                var resistance = m_resistances.SingleOrDefault((r) => r.Element == elementalResistance.Key);
                if (resistance != null)
                {
                    resistance.Preview(elementalResistance.Value);
                }
            }
        }

        private void PreviewInternal(Dictionary<StatusEffect, float> statusEffectResistances)
        {
            foreach (var statusEffectResistance in statusEffectResistances)
            {
                var resistance = m_resistances.SingleOrDefault((r) => r.Status == statusEffectResistance.Key);
                if (resistance != null)
                {
                    resistance.Preview(statusEffectResistance.Value);
                }
            }
        }
    }
}
