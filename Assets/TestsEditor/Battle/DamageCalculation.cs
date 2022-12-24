using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        // Test that the damage formula returns the expected values
        // Calculated without variance
        [Test]
        [TestCase("A0001", 7, 43)]
        [TestCase("M0001", 47, 3)]
        public void Apply_Damage_Should_Succeed(string abilityID, int expectedDamage, int exepectedHealth)
        {
            var ability = Abilities.FirstOrDefault(ability => ability.Id == abilityID);
            var effect = ability.Effects[0];
            var attackValue = DamageCalculation.GetOffensivePotencyValue(Character, effect);
            var defenseValue = DamageCalculation.GetDefensivePotencyValue(Enemy, effect);

            var damage = DamageCalculation.GetDamageCalculationEquationResult(attackValue, defenseValue);

            Assert.AreEqual(expectedDamage, damage);

            Enemy.ApplyAttributeModification(Attribute.HP, damage * -1);

            Assert.AreEqual(exepectedHealth, Enemy.GetAttribute(Attribute.HP));
        }

        // A Test behaves as an ordinary method
        [Test]
        [TestCase("A0002", 19, 31)]
        [TestCase("A0003", 37, 13)]
        public void Apply_Damage_With_Piercing_Damage_Should_Succeed(string abilityID, int expectedDamage, int expectedHealth)
        {
            var ability = Abilities.FirstOrDefault(ability => ability.Id == abilityID);
            var effect = ability.Effects[0];
            var attackValue = DamageCalculation.GetOffensivePotencyValue(Character, effect);
            var defenseValue = DamageCalculation.GetDefensivePotencyValue(Enemy, effect);

            var damage = DamageCalculation.GetDamageCalculationEquationResult(attackValue, defenseValue);

            Assert.AreEqual(expectedDamage, damage);

            Enemy.ApplyAttributeModification(Attribute.HP, damage * -1);

            Assert.AreEqual(expectedHealth, Enemy.GetAttribute(Attribute.HP));
        }
    }
}