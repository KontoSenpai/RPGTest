using RPGTest.Enums;
using RPGTest.Models.Abilities;
using RPGTest.Models.Effects;
using RPGTest.Models.Entity.Components;
using RPGTest.Models.Entity.Extensions;
using RPGTest.Modules.Battle;
using RPGTest.Modules.Battle.Action;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;
using Attribute = RPGTest.Enums.Attribute;

namespace RPGTest.Models.Entity
{
    // Special EventArgs class to hold info about Shapes.
    public class BuffsRefreshedArgs : EventArgs
    {
        public BuffsRefreshedArgs(IEnumerable<Effect> buffs, IEnumerable<Effect> debuffs)
        {
            Buffs = buffs;
            Debuffs = debuffs;
        }

        public IEnumerable<Effect> Buffs { get; }

        public IEnumerable<Effect> Debuffs { get; }
    }

    public abstract partial class Entity : IdObject
    {
        #region properties
        public GameObject BattleModel { get; set; }
        
        public string Surname { get; set; }

        public string Title { get; set; }

        public int Level { get; set; }

        public bool FrontRow { get; set; }

        public int CurrentHP { get; set; }

        public int CurrentMP { get; set; }

        public int CurrentStamina { get; set; }

        [YamlIgnore]
        public bool IsAlive { get { return CurrentHP > 0; } }

        [YamlIgnore]
        public EffectsComponent EffectsComponent { get; set; } = new EffectsComponent();

        public string DefaultAttack { get; set; }

        public Attributes BaseAttributes { get; set; }

        public Stance Stance { get; set; } = new Stance();

        public List<Buff> Buffs { get; set; } = new List<Buff>();

        public List<Status> StatusEffects { get; set; } = new List<Status>();

        public Dictionary<Buff, int> StatChangeResistance { get; set; } = new Dictionary<Buff, int>();

        public StatusEffectsResistances StatusEffectResistances { get; set; } = new StatusEffectsResistances();

        public ElementalResistances ElementalResistances { get; set; } = new ElementalResistances();

        public virtual Range PowerRange { get; set; } = new Range() { Min = 0.75f, Max = 1.25f };
        #endregion

        #region event handling
        protected virtual void OnBuffsRefreshed(BuffsRefreshedArgs e)
        {
            BuffsRefreshed?.Invoke(this, e);
        }
        public event EventHandler<BuffsRefreshedArgs> BuffsRefreshed;
        #endregion

        #region variables
        protected float m_currentATB = 0.0f;

        private readonly float m_baseMaxATB = 1000.0f;
        protected float m_maxATB = 1000.0f;
        #endregion

        public float GetATB()
        {
            return m_currentATB;
        }

        public virtual bool FillATB()
        {
            m_currentATB += GetAttribute(Attribute.Speed);
            return m_currentATB >= m_maxATB;
        }

        public abstract IEnumerator SelectAction(BattleManager manager, List<PlayableCharacter> playerParty, List<Enemy> enemyParty, System.Action<ActionSequence> selectedAction);

        public virtual void ResetATB(int variation = 0)
        {
            m_currentATB = 0;
            m_maxATB = m_baseMaxATB + variation;
        }

        public virtual void RefillResources() {}

        public void PerformAction()
        {
            m_currentATB = 0;
        }

        public virtual float GetPowerRangeValue()
        {
            return PowerRange.GetValue();
        }

        public virtual bool IsAbilityCastable(Ability ability)
        {
            foreach (var castCost in ability.CastCost)
            {
                var cost = System.Math.Abs(castCost.Value);
                switch (castCost.Key)
                {
                    case Attribute.MaxHP:
                    case Attribute.HP:
                        if (GetAttribute(Attribute.HP) < cost)
                        {
                            return false;
                        }
                        break;
                    case Attribute.MaxMP:
                    case Attribute.MP:
                        if (GetAttribute(Attribute.MP) < cost)
                        {
                            return false;
                        }
                        break;
                    case Attribute.MaxStamina:
                    case Attribute.Stamina:
                        if (GetAttribute(Attribute.Stamina) < cost)
                        {
                            return false;
                        }
                        break;
                }
            }
            return true;
        }

        public virtual float GetAttribute(Attribute attribute)
        {
            GetAttributes().TryGetValue(attribute, out float value);
            return value;
        }

        public virtual Dictionary<Attribute, float> GetAttributes()
        {
            var hpPercentage = CurrentHP / BaseAttributes.MaxHP;
            var mpPercentage = BaseAttributes.MaxMP > 0 ? CurrentMP / BaseAttributes.MaxMP : 1.0f;
            var staminaPercentage = CurrentStamina / BaseAttributes.MaxStamina;

            Dictionary<Attribute, float> attributes = new Dictionary<Attribute, float>
            {
                // Computed Attributes
                { Attribute.HPPercentage, CurrentHP / BaseAttributes.MaxHP },
                { Attribute.MPPercentage, BaseAttributes.MaxMP > 0 ? CurrentMP / BaseAttributes.MaxMP : 1.0f },
                { Attribute.StaminaPercentage, CurrentStamina / BaseAttributes.MaxStamina },

                // Base Attributes
                { Attribute.MaxHP, GetAttributeValue(Attribute.MaxHP, BaseAttributes.MaxHP ) },
                { Attribute.MaxMP,  GetAttributeValue(Attribute.MaxMP, BaseAttributes.MaxMP ) },
                { Attribute.MaxStamina, GetAttributeValue(Attribute.MaxStamina, BaseAttributes.MaxStamina ) },

                { Attribute.Attack, GetAttributeValue(Attribute.Attack, BaseAttributes.Attack) },
                { Attribute.Defense, GetAttributeValue(Attribute.Defense, BaseAttributes.Defense) },

                { Attribute.Magic, GetAttributeValue(Attribute.Magic, BaseAttributes.Magic) },
                { Attribute.Resistance, GetAttributeValue(Attribute.Resistance, BaseAttributes.Resistance) },

                { Attribute.Speed, GetAttributeValue(Attribute.Speed, BaseAttributes.Speed) },

                { Attribute.Hit, GetAttributeValue(Attribute.Hit, BaseAttributes.Hit) },

                { Attribute.Block, GetAttributeValue(Attribute.Block, 0.1f) },
            };

            attributes.Add(Attribute.HP, attributes[Attribute.MaxHP] * hpPercentage);
            attributes.Add(Attribute.MP, attributes[Attribute.MaxMP] * mpPercentage);
            attributes.Add(Attribute.Stamina, attributes[Attribute.MaxStamina] * staminaPercentage);

            return attributes;
        }

        private float GetAttributeValue(Attribute attribute, float baseValue)
        {
            return Mathf.Ceil(baseValue * EffectsComponent.GetHighestAttributeBuff(attribute) / EffectsComponent.GetHighestAttributeDebuff(attribute));
        }

        public virtual Dictionary<Element, float> GetElementalResistances()
        {
            Dictionary<Element, float> elementalResistances = new Dictionary<Element, float>();

            elementalResistances.Add(Element.Fire, ElementalResistances.Fire + EffectsComponent.GetElementResistanceVariation(Element.Fire));
            elementalResistances.Add(Element.Ice, ElementalResistances.Ice + EffectsComponent.GetElementResistanceVariation(Element.Ice));
            elementalResistances.Add(Element.Water, ElementalResistances.Water + EffectsComponent.GetElementResistanceVariation(Element.Water));
            elementalResistances.Add(Element.Lightning, ElementalResistances.Lightning + EffectsComponent.GetElementResistanceVariation(Element.Lightning));
            elementalResistances.Add(Element.Wind, ElementalResistances.Wind + EffectsComponent.GetElementResistanceVariation(Element.Wind));
            elementalResistances.Add(Element.Earth, ElementalResistances.Earth + EffectsComponent.GetElementResistanceVariation(Element.Earth));
            elementalResistances.Add(Element.Light, ElementalResistances.Light + EffectsComponent.GetElementResistanceVariation(Element.Light));
            elementalResistances.Add(Element.Dark, ElementalResistances.Dark + EffectsComponent.GetElementResistanceVariation(Element.Dark));

            return elementalResistances;
        }

        public virtual Dictionary<StatusEffect, float> GetStatusEffectResistances()
        {
            Dictionary<StatusEffect, float> statuEffectResistances = new Dictionary<StatusEffect, float>();

            statuEffectResistances.Add(StatusEffect.Blind, StatusEffectResistances.Blind + EffectsComponent.GetStatusEffectResistanceVariation(StatusEffect.Blind));
            statuEffectResistances.Add(StatusEffect.Bleed, StatusEffectResistances.Bleed + EffectsComponent.GetStatusEffectResistanceVariation(StatusEffect.Bleed));
            statuEffectResistances.Add(StatusEffect.Poison, StatusEffectResistances.Poison + EffectsComponent.GetStatusEffectResistanceVariation(StatusEffect.Poison));
            statuEffectResistances.Add(StatusEffect.Paralysis, StatusEffectResistances.Paralysis + EffectsComponent.GetStatusEffectResistanceVariation(StatusEffect.Paralysis));
            statuEffectResistances.Add(StatusEffect.Silence, StatusEffectResistances.Silence + EffectsComponent.GetStatusEffectResistanceVariation(StatusEffect.Silence));
            statuEffectResistances.Add(StatusEffect.Confusion, StatusEffectResistances.Confusion + EffectsComponent.GetStatusEffectResistanceVariation(StatusEffect.Confusion));
            statuEffectResistances.Add(StatusEffect.Freeze, StatusEffectResistances.Freeze + EffectsComponent.GetStatusEffectResistanceVariation(StatusEffect.Freeze));
            statuEffectResistances.Add(StatusEffect.Slow, StatusEffectResistances.Slow + EffectsComponent.GetStatusEffectResistanceVariation(StatusEffect.Slow));

            return statuEffectResistances;
        }


        #region Modifications
        /// <summary>
        /// Apply an attribute modification to an Entity
        /// </summary>
        /// <param name="attribute">Attribute targeted by the modification</param>
        /// <param name="value">Amount that should be modified</param>
        public virtual void ApplyAttributeModification(Attribute attribute, int value)
        {
            switch (attribute)
            {
                case Attribute.MaxHP:
                case Attribute.HP:
                    CurrentHP += value;
                    if (CurrentHP < 0)
                    {
                        CurrentHP = 0;
                    }
                    else if (CurrentHP > BaseAttributes.MaxHP)
                    {
                        CurrentHP = BaseAttributes.MaxHP;
                    }
                    break;
                case Attribute.MaxMP:
                case Attribute.MP:
                    CurrentMP += value;
                    if (CurrentMP < 0)
                    {
                        CurrentMP = 0;
                    }
                    else if (CurrentMP > BaseAttributes.MaxMP)
                    {
                        CurrentMP = BaseAttributes.MaxMP;
                    }
                    break;
                case Attribute.MaxStamina:
                case Attribute.Stamina:
                    CurrentStamina += value;
                    if (CurrentStamina < 0)
                    {
                        CurrentStamina = 0;
                    }
                    else if (CurrentStamina > BaseAttributes.MaxStamina)
                    {
                        CurrentStamina = BaseAttributes.MaxStamina;
                    }
                    break;
            }
        }

        public virtual bool ApplyStatusEffect(StatusEffect effect, int potency)
        {
            return false;
        }

        public virtual void RemoveStatusEffect(StatusEffect statusEffect, RemovalType removalType)
        {
            StatusEffects.RemoveAll(s => s.RemovalType == removalType);
            return;
        }
        #endregion
    }
}
