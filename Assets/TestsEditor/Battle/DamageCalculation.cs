using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Modules.Battle;
using UnityEngine;
using UnityEngine.TestTools;

namespace TestsEditor.Battle
{
    public class TestDamageCalculation : BattleFixture
    {
        #region Fixture
        Buff buffAttack = new Buff
        {
            Attribute = Attribute.Attack,
            Duration = 2,
            Value = .5f,
            RemovalType = RemovalType.None
        };

        Buff buffDefense = new Buff
        {
            Attribute = Attribute.Defense,
            Duration = 5,
            Value = .5f,
            RemovalType = RemovalType.None
        };

        Effect[] testEffects = {
        new Effect // Attack no piercing
        {
            Type = EffectType.Damage,
            Potency = new EffectPotency
            {
                Attribute = Attribute.HP,
            },
            Scalings = new Dictionary<Attribute, float>
            {
                { Attribute.Attack, 1.0f }
            }
        },
        new Effect // Attack, partial piercing
        {
            Type = EffectType.Damage,
            Potency = new EffectPotency
            {
                Attribute = Attribute.HP,
                IgnoreDefense = .6f,
            },
            Scalings = new Dictionary<Attribute, float>
            {
                { Attribute.Attack, 1.0f }
            }
        },
        new Effect // Attack, full piercing
        {
            Type = EffectType.Damage,
            Potency = new EffectPotency
            {
                Attribute = Attribute.HP,
                IgnoreDefense = 1f,
            },
            Scalings = new Dictionary<Attribute, float>
            {
                { Attribute.Attack, 1.0f }
            }
        },
        new Effect // Magic, no piercing
        {
            Type = EffectType.Damage,
            Potency = new EffectPotency
            {
                Attribute = Attribute.HP,
            },
            Scalings = new Dictionary<Attribute, float>
            {
                { Attribute.Magic, 1.0f }
            }
        }
    };

        #endregion


        // Test that the damage formula returns the expected values
        // Calculated without variance
        [Test]
        [TestCase(0, 7, 43)]
        [TestCase(3, 47, 3)]
        public void Apply_Damage_Should_Succeed(int effectIndex, int expectedDamage, int exepectedHealth)
        {
            var effect = testEffects[effectIndex];
            var attackValue = DamageCalculation.GetOffensivePotencyValue(Character, effect);
            var defenseValue = DamageCalculation.GetDefensivePotencyValue(Enemy, effect);

            var damage = DamageCalculation.GetDamageCalculationEquationResult(attackValue, defenseValue);

            Assert.AreEqual(expectedDamage, damage);

            Enemy.ApplyAttributeModification(Attribute.HP, damage * -1);

            Assert.AreEqual(exepectedHealth, Enemy.GetAttribute(Attribute.HP));
        }

        // A Test behaves as an ordinary method
        [Test]
        [TestCase(1, 19, 31)]
        [TestCase(2, 37, 13)]
        public void Apply_Damage_With_Piercing_Damage_Should_Succeed(int effectIndex, int expectedDamage, int expectedHealth)
        {
            var effect = testEffects[effectIndex];
            var attackValue = DamageCalculation.GetOffensivePotencyValue(Character, effect);
            var defenseValue = DamageCalculation.GetDefensivePotencyValue(Enemy, effect);

            var damage = DamageCalculation.GetDamageCalculationEquationResult(attackValue, defenseValue);

            Assert.AreEqual(expectedDamage, damage);

            Enemy.ApplyAttributeModification(Attribute.HP, damage * -1);

            Assert.AreEqual(expectedHealth, Enemy.GetAttribute(Attribute.HP));
        }
    }
}