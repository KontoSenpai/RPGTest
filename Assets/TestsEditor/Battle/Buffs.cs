using NUnit.Framework;
using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Entity;
using System.Collections.Generic;
using System.Linq;

namespace TestsEditor.Battle
{
    public class Buffs : BattleFixture
    {
        [Test]
        [TestCase("B0001")]
        public void Add_Buff_Should_Succeed(string abilityID)
        {
            var ability = Abilities.FirstOrDefault(ability => ability.Id == abilityID);
            var toto = new MockEntityAction(Character, ActionType.Ability, ability, new List<Entity> { Character });
                       
            //Character.AddBuff(buffAttack);
            Assert.IsNotNull(Character.GetHighestAttributeBuff(RPGTest.Enums.Attribute.Attack));
        }

        [Test]
        public void Add_Buff_Should_Become_New_Highest()
        {
            //Character.AddBuff(buffAttack);
            //Character.AddBuff(buffAttack2);

            var value = Character.GetHighestAttributeBuff(RPGTest.Enums.Attribute.Attack);

            Assert.AreEqual(2f, value);
        }

        [Test]
        public void Buff_Value_Should_Apply()
        {
            //Character.AddBuff(buffAttack);
            Assert.AreEqual(15, Character.GetOffensiveAttribute(RPGTest.Enums.Attribute.Attack));
        }

        [Test]
        public void Buff_Refresh_Should_Succeed()
        {
            //Character.AddBuff(buffAttack);
            Character.ReduceStatusDurations();

            Assert.AreEqual(1, Character.Buffs[0].Duration);

            Character.ReduceStatusDurations();

            Assert.AreEqual(0, Character.Buffs.Count);
        }
    }
}