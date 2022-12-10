﻿using RPGTest.Enums;
using System.Collections.Generic;

namespace RPGTest.Models
{
    public class EffectPotency
    {
        public float Potency { get; set; }

        public float IgnoreDefense { get; set; } = 0.0f;

        public RemovalType RemovalType { get; set; } = RemovalType.None;

        public int Duration { get; set; }
    }

    public class Effect
    {
        public EffectType EffectType { get; set; }

        public Dictionary<Attribute, EffectPotency> Attributes { get; set; } = new Dictionary<Attribute, EffectPotency>();

        public Dictionary<Attribute, float> Scalings { get; set; } = new Dictionary<Attribute, float>();

        public Dictionary<StatusEffect, EffectPotency> Statuses { get; set; } = new Dictionary<StatusEffect, EffectPotency>();

        public Dictionary<Element, int> Elements { get; set; }

        public TargetType TargetType { get; set; } = TargetType.None;

        public List<float> HitFrames { get; set; } = new List<float> { 0 };

        public List<float> HitPower { get; set; } = new List<float> { 1 };

        public Range PowerRange { get; set; } = new Range();

        public bool EvaluateEffect(Dictionary<Attribute, float> entityAttributes)
        {
            bool valid = false;
            switch(EffectType)
            {
                case EffectType.Heal:
                    foreach (var attribute in Attributes)
                    {
                        switch (attribute.Key)
                        {
                            case Attribute.MaxHP:
                                valid = entityAttributes[Attribute.HPPercentage] < 1.0f;
                                break;
                            case Attribute.MaxMP:
                                valid = entityAttributes[Attribute.HPPercentage] < 1.0f;
                                break;
                        }
                    }
                    break;
            }
            return valid;
        }
    }
}
