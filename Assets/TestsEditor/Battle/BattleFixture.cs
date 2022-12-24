using NUnit.Framework;
using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Abilities;
using RPGTest.Models.Entity;
using RPGTest.Modules.Battle.Action;
using System.Collections.Generic;

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
                Effects = new List<Effect>
                {
                    new Effect {
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
                }
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
                Effects = new List<Effect>
                {
                    new Effect
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
                },
            },
            new Ability
            {
                Id = "A0003",
                Name = "Attack full Piercing",
                AbilityType = AbilityType.Weapon,
                Effects = new List<Effect>
                {
                    new Effect
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
                },
            },
            new Ability
            {
                Id = "M0001",
                Name = "Magic no piercing",
                AbilityType = AbilityType.AttackMagic,
                Effects = new List<Effect>
                {
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
                        },
                    }
                },
            },
            new Ability
            {
                Id = "B0001",
                Name = "Buff Attack",
                AbilityType = AbilityType.SupportMagic,
                Effects = new List<Effect>
                {
                    new Effect
                    {
                        Type = EffectType.Buff,
                        Potency = new EffectPotency
                        {
                            Attribute = Attribute.Attack,
                            Potency = 50,
                        },
                    }
                }
            },
            new Ability
            {
                Id = "B0002",
                Name = "Buff Defense",
                AbilityType = AbilityType.SupportMagic,
                Effects = new List<Effect>
                {
                    new Effect
                    {
                        Type = EffectType.Buff,
                        Potency = new EffectPotency
                        {
                            Attribute = Attribute.Defense,
                            Potency = 50,
                        },
                    }
                }
            },
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

            Character.PlayerBuffsRefreshed += OnBuffApplied;
        }

        private void OnBuffApplied(List<Buff> buffs)
        {
        }
    }
}
