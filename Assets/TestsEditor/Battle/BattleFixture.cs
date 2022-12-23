using NUnit.Framework;
using RPGTest.Models;
using RPGTest.Models.Entity;
using System.Collections.Generic;

namespace TestsEditor.Battle
{
    public class BattleFixture
    {
        public PlayableCharacter Character { get; set; }
        public Enemy Enemy { get; set; }

        [SetUp]
        public void Setup()
        {
            Character = new PlayableCharacter
            {
                BaseAttributes = new Attributes
                {
                    MaxHP = 50,
                    Attack = 10,
                    Magic = 40,
                }
            };
            Character.CurrentHP = Character.BaseAttributes.MaxHP;

            Enemy = new Enemy
            {
                BaseAttributes = new Attributes
                {
                    MaxHP = 50,
                    Defense = 5,
                    Resistance = 8,
                }
            };
            Enemy.CurrentHP = Enemy.BaseAttributes.MaxHP;

            Character.PlayerBuffsRefreshed += OnBuffApplied;
        }

        private void OnBuffApplied(List<Buff> buffs)
        {
        }
    }
}
