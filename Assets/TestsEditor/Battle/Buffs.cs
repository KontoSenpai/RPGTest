using NUnit.Framework;
using RPGTest.Models;
using RPGTest.Models.Entity;
using System.Collections.Generic;

namespace TestsEditor.Battle
{
    public class Buffs : BattleFixture
    {
        Buff buffAttack = new Buff
        {
            Attribute = RPGTest.Enums.Attribute.Attack,
            Duration = 2,
            Value = .5f,
            RemovalType = RPGTest.Enums.RemovalType.None
        };

        Buff buffAttack2 = new Buff
        {
            Attribute = RPGTest.Enums.Attribute.Attack,
            Duration = 1,
            Value = 1f,
            RemovalType = RPGTest.Enums.RemovalType.None
        };

        [Test]
        public void Add_Buff_Should_Succeed()
        {
            Character.AddBuff(buffAttack);
            Assert.IsNotNull(Character.GetHighestBuff(RPGTest.Enums.Attribute.Attack));
        }

        [Test]
        public void Add_Buff_Should_Become_New_Highest()
        {
            Character.AddBuff(buffAttack);
            Character.AddBuff(buffAttack2);

            var value = Character.GetHighestBuff(RPGTest.Enums.Attribute.Attack);

            Assert.AreEqual(2f, value);
        }

        [Test]
        public void Buff_Value_Should_Apply()
        {
            Character.AddBuff(buffAttack);
            Assert.AreEqual(15, Character.GetOffensiveAttribute(RPGTest.Enums.Attribute.Attack));
        }

        [Test]
        public void Buff_Refresh_Should_Succeed()
        {
            Character.AddBuff(buffAttack);
            Character.ReduceStatusDurations();

            Assert.AreEqual(1, Character.Buffs[0].Duration);

            Character.ReduceStatusDurations();

            Assert.AreEqual(0, Character.Buffs.Count);
        }
    }
}