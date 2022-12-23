using RPGTest.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.Models
{
    public class EffectPotency
    {
        public Attribute Attribute { get; set; } = Attribute.None;

        public StatusEffect StatusEffect { get; set; } = StatusEffect.None;

        public Element Element { get; set; } = Element.None;

        public float Potency { get; set; } = 1.0f;

        public float IgnoreDefense { get; set; } = 0.0f;

        public RemovalType RemovalType { get; set; } = RemovalType.None;

        public int Duration { get; set; } = 0;
    }

    public class Effect
    {
        public EffectType Type { get; set; }

        public EffectPotency Potency { get; set; }

        public Dictionary<Attribute, float> Scalings { get; set; } = new Dictionary<Attribute, float>();

        public TargetType TargetType { get; set; } = TargetType.None;

        public List<float> HitFrames { get; set; } = new List<float> { 0 };

        public List<float> HitPower { get; set; } = new List<float> { 1 };

        public Range PowerRange { get; set; } = new Range();

        public bool EvaluateEffect(Dictionary<Attribute, float> entityAttributes)
        {
            bool valid = false;
            switch (Type)
            {
                case EffectType.Heal:
                    switch (Potency.Attribute)
                    {
                        case Attribute.MaxHP:
                            valid = entityAttributes[Attribute.HPPercentage] < 1.0f;
                            break;
                        case Attribute.MaxMP:
                            valid = entityAttributes[Attribute.HPPercentage] < 1.0f;
                            break;
                    }
                    break;
            }
            return valid;
        }
    }

    public class Ability : IdObject
    {
        public string Description { get; set; }

        public AbilityType AbilityType { get; set; } = AbilityType.Weapon;

        public List<EquipmentType> EquipmentRestrictrion { get; set; }

        public Dictionary<Attribute, int> Requisites { get; set; }

        public double HitRate { get; set; } = 1.0f;

        public double CriticalRate { get; set; } = 1.0;

        public List<Effect> Effects { get; set; }

        public List<TargetType> TargetTypes { get; set; }

        public TargetType DefaultTarget { get; set; }

        public Dictionary<Attribute, int> CastCost { get; set; } = new Dictionary<Attribute, int>();

        public float CastTime { get; set; } = 1.0f;

        public float Backswing { get; set; } = 0.5f;
    }

    public class Range
    {
        public float Min { get; set; }

        public float Max { get; set; }

        public float GetValue()
        {
            return Min == 0 && Max == 0 ? 1.0f : Random.Range(Min, Max);
        }
    }
}
