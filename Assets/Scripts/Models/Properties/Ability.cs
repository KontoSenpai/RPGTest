using RPGTest.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.Models
{
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
