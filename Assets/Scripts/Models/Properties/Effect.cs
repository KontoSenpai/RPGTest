using RPGTest.Enums;
using System.Collections.Generic;

namespace RPGTest.Models
{
    public class EffectPotency
    {
        public float Potency { get; set; }

        public int TurnInEffect { get; set; }
    }

    public class Effect
    {
        public EffectType EffectType { get; set; }

        public Dictionary<Attribute, EffectPotency> Attributes { get; set; } = new Dictionary<Attribute, EffectPotency>();

        public Dictionary<string, float> Scalings { get; set; } = new Dictionary<string, float>();

        public Dictionary<StatusEffect, EffectPotency> Statuses { get; set; }

        public Dictionary<Element, int> Elements { get; set; }

        public TargetType TargetType { get; set; } = TargetType.None;

        public List<float> HitFrames { get; set; } = new List<float> { 0 };

        public List<float> HitPower { get; set; } = new List<float> { 1 };

        public Range PowerRange { get; set; } = new Range();

        public bool Dispellable { get; set; }

        public bool EvaluateEffect(Dictionary<string, float> entityAttributes)
        {
            bool valid = false;
            switch(EffectType)
            {
                case EffectType.Cure:
                    foreach (var attribute in Attributes)
                    {
                        switch (attribute.Key)
                        {
                            case Attribute.HP:
                                valid = entityAttributes["HPPercentage"] < 1.0f;
                                break;
                            case Attribute.MP:
                                valid = entityAttributes["MPPercentage"] < 1.0f;
                                break;
                        }
                    }
                    break;
            }
            return valid;
        }
    }
}
