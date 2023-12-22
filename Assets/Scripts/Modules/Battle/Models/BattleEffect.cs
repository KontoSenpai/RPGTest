using RPGTest.Enums;
using RPGTest.Models.Entity;
using RPGTest.Modules.Battle.Action;
using System.Collections.Generic;

namespace RPGTest.Modules.Battle.Models
{
    public class BattleEffect
    {
        public string Id { get; set; }

        public ActionType ActionType { get; set; }

        public EffectType EffectType { get; set; }

        public Entity Target { get; set; }

        public Attribute Attribute { get; set; } = Attribute.None;

        public StatusEffect StatusEffect { get; set; }

        public int Value { get; set; }

        public int Duration { get; set; } = 0;

        public RemovalType RemovalType { get; set; } = RemovalType.None;

        public List<float> Frames { get; set; } = new List<float> { 0 };

        public List<float> HitPower { get; set; } = new List<float> { 1 };

        public ActionState State { get; set; } = ActionState.Pending;
    }
}