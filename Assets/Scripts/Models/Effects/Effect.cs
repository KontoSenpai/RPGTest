using RPGTest.Enums;
using RPGTest.Modules.Battle.Action;
using System.Collections.Generic;
using Attribute = RPGTest.Enums.Attribute;

namespace RPGTest.Models.Effects
{
    public class EffectEvaluation
    {
        public string Id { get; set; }

        public ActionType ActionType { get; set; }

        public EffectType EffectType { get; set; }

        public Entity.Entity Target { get; set; }

        public Attribute Attribute { get; set; } = Attribute.None;

        public StatusEffect StatusEffect { get; set; }

        public int Value { get; set; }

        public int Duration { get; set; } = 0;

        public RemovalType RemovalType { get; set; } = RemovalType.None;

        public List<float> Frames { get; set; } = new List<float> { 0 };

        public List<float> HitPower { get; set; } = new List<float> { 1 };

        public ActionState State { get; set; } = ActionState.Pending;
    }

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

    public partial class Effect : IdObject
    {
        public string Label { get; set; }

        public string Icon { get; set; }

        public EffectType Type { get; set; }

        public EffectPotency Potency { get; set; }

        public Dictionary<Attribute, float> Scalings { get; set; } = new Dictionary<Attribute, float>();

        public TargetType TargetType { get; set; } = TargetType.None;

        public List<float> HitFrames { get; set; } = new List<float> { 0 };

        public List<float> HitPower { get; set; } = new List<float> { 1 };

        public Range PowerRange { get; set; } = new Range();

        /// <summary>
        /// Determine if the given attributes are valid for any potential modifications
        /// </summary>
        /// <param name="entityAttributes">Attributes to evaluates</param>
        /// <returns></returns>
        public bool EvaluateEffectAttributes(Dictionary<Attribute, float> entityAttributes)
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
}
