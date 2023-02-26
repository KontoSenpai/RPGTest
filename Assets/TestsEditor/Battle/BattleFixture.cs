using NUnit.Framework;
using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Abilities;
using RPGTest.Models.Effects;
using RPGTest.Models.Entity;
using RPGTest.Modules.Battle.Action;
using System.Collections.Generic;
using Attribute = RPGTest.Enums.Attribute;

namespace TestsEditor.Battle
{
    public class MockEntityAction : EntityAction
    {
        public MockEntityAction(
            Entity caster,
            ActionType actionType,
            Ability ability,
            List<Entity> targets
        ) 
            : base(caster, actionType, ability.Id, targets, null)
        {}

    }
    public class BattleFixture
    {
        public PlayableCharacter Character { get; set; }
        public Enemy Enemy { get; set; }

        public Ability[] Abilities { get; set; } =
        {
            new Ability
            {
                Id = "A0001",
                Name = "Attack no Piercing",
                AbilityType = AbilityType.Weapon,
                Effects = new List<string>
                {
                    "D0001",
                },
            },
            new Ability
            {
                Id = "A0002",
                Name = "Attack partial Piercing",
                AbilityType = AbilityType.Weapon,
                EquipmentRestrictrion = new List<EquipmentType>
                {
                    EquipmentType.Spear
                },
                Effects = new List<string>
                {   
                    "D0001",
                },
            },
            new Ability
            {
                Id = "A0003",
                Name = "Attack full Piercing",
                AbilityType = AbilityType.Weapon,
                Effects = new List<string>
                {
                    "D0003",
                },
            },
            new Ability
            {
                Id = "M0001",
                Name = "Magic no piercing",
                AbilityType = AbilityType.AttackMagic,
                Effects = new List<string>
                {
                    "D0004",
                },
            },
            new Ability
            {
                Id = "B0001",
                Name = "Buff Attack",
                AbilityType = AbilityType.SupportMagic,
                Effects = new List<string>
                {
                    "B0001",
                }
            },
            new Ability
            {
                Id = "B0002",
                Name = "Buff Defense",
                AbilityType = AbilityType.SupportMagic,
                Effects = new List<string>
                {
                    "B0002",
                }
            },
        };

        public Effect[] Effects { get; set; } =
        {
            new Effect
            {
                Id = "D0001",
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
             new Effect
             {
                Id = "D0002",
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
            new Effect
            {
                Id = "D0003",
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
                Id = "D0004",
                Type = EffectType.Damage,
                Potency = new EffectPotency
                {
                    Attribute = Attribute.HP,
                },
                Scalings = new Dictionary<Attribute, float>
                {
                    { Attribute.Magic, 1.0f }
                },
            },
            new Effect
            {   
                Id = "B0001",
                Type = EffectType.Buff,
                Potency = new EffectPotency
                {
                    Attribute = Attribute.Attack,
                    Potency = 50,
                },
            },
            new Effect
            {
                Id = "B0002",
                Type = EffectType.Buff,
                Potency = new EffectPotency
                {
                    Attribute = Attribute.Defense,
                    Potency = 50,
                },
            }
        };

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

            Character.BuffsRefreshed += OnBuffApplied;
        }

        private void OnBuffApplied(object sender, BuffsRefreshedArgs e)
        {

        }
    }
}
