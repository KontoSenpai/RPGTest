using System.Collections.Generic;
using System.Linq;
using RPGTest.Models.Effects;
using RPGTest.Models.Entity;
using RPGTest.Models.Entity.Extensions;
using UnityEngine;

namespace RPGTest.Modules.Battle
{
    public static class DamageCalculation
    {
        private static float m_offensiveCoefficient = 1.5f;
        private static float m_defensiveCoefficient = 0.1f;

        private static Dictionary<int, float> m_differenceCoefficient = new Dictionary<int, float>
        {
            {   0,  0.0f },
            {   5,  .05f },
            {  15,  .1f  },
            {  30,  .15f },
            {  50,  .2f  },
            {  70,  .25f },
            {  95,  .30f },
            { 115,  .35f },
            { 135,  .4f  },
            { 160,  .45f },
            { 190,  .5f  },
        };

        public static int GetOffensivePotencyValue(Entity entity, Effect effect)
        {
            var scalingAmount = effect.Scalings.Count;
            float attackPower = 0.0f;
            foreach(var scaling in effect.Scalings)
            {
                attackPower += entity.GetOffensiveAttribute(scaling.Key) * scaling.Value / scalingAmount;
            }
            return Mathf.CeilToInt(Mathf.Pow(attackPower, m_offensiveCoefficient));
        }

        public static int GetDefensivePotencyValue(Entity entity, Effect effect)
        {
            var scalingAmount = effect.Scalings.Count;
            float defensePower = 0.0f;
            foreach (var scaling in effect.Scalings)
            {
                defensePower += entity.GetDefensiveAttribute(scaling.Key) / scalingAmount;
            }

            defensePower = defensePower * (1 - effect.Potency.IgnoreDefense);
            return Mathf.CeilToInt(defensePower);
        }

        public static int GetDamageCalculationEquationResult(int attackPower, int defensePower)
        {
            var difference = attackPower - defensePower;
            var diffCoefficient = GetDifferenceCoefficient(difference);

            return Mathf.CeilToInt((attackPower / (defensePower == 0 ? 1 : defensePower)) * diffCoefficient);
        }

        private static float GetDifferenceCoefficient(int value)
        {
            var coeff = m_differenceCoefficient.LastOrDefault(c => c.Key <= Mathf.Abs(value)).Value;

            coeff = value > 0 ? 1 + coeff : 1 - coeff;

            return coeff;
        }

    }
}
