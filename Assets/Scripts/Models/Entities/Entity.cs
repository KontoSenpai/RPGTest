using RPGTest.Enums;
using RPGTest.Models.Abilities;
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
        public BuffsRefreshedArgs(List<Buff> buffs)
        {
            Buffs = buffs;
        }

        public List<Buff> Buffs { get; }
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

        public string DefaultAttack { get; set; }

        public Attributes BaseAttributes { get; set; }

        public Stance Stance { get; set; } = new Stance();

        public List<Buff> Buffs { get; set; } = new List<Buff>();

        public List<Status> StatusEffects { get; set; } = new List<Status>();

        public Dictionary<Buff, int> StatChangeResistance { get; set; } = new Dictionary<Buff, int>();

        public Dictionary<StatusEffect, int> StatusEffectResistance { get; set; } = new Dictionary<StatusEffect, int>();

        public Dictionary<Element, int> ElementResistance { get; set; } = new Dictionary<Element, int>();

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
            m_currentATB += GetAttribute(Attribute.TotalSpeed);
            return m_currentATB >= m_maxATB;
        }

        public abstract IEnumerator SelectAction(BattleManager manager, List<PlayableCharacter> playerParty, List<Enemy> enemyParty, System.Action<ActionSequence> selectedAction);

        public virtual void ResetATB(int variation = 0)
        {
            m_currentATB = 0;
            m_maxATB = m_baseMaxATB + variation;
        }

        public virtual void RefillResources() {}

        public virtual void ReduceStatusDurations()
        {
            foreach(var b in Buffs)
            {
                b.Duration--;
            }
            Buffs.RemoveAll(b => b.Duration <= 0);
            OnBuffsRefreshed(new BuffsRefreshedArgs(Buffs));
        }

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
            Dictionary<Attribute, float> attributes = new Dictionary<Attribute, float>();

            // Base Attributes
            attributes.Add(Attribute.HP, CurrentHP);
            attributes.Add(Attribute.MaxHP, BaseAttributes.MaxHP);

            attributes.Add(Attribute.MP, CurrentMP);
            attributes.Add(Attribute.MaxMP, BaseAttributes.MaxMP);

            attributes.Add(Attribute.Stamina, CurrentStamina);
            attributes.Add(Attribute.MaxStamina, BaseAttributes.MaxStamina);

            attributes.Add(Attribute.Attack, BaseAttributes.Attack);
            attributes.Add(Attribute.Defense, BaseAttributes.Defense);

            attributes.Add(Attribute.Magic, BaseAttributes.Magic);
            attributes.Add(Attribute.Resistance, BaseAttributes.Resistance);

            attributes.Add(Attribute.Speed, BaseAttributes.Speed);

            attributes.Add(Attribute.Hit, BaseAttributes.Hit);
            //attributes.Add(Attribute.Block);

            // Computed Attributes
            attributes.Add(Attribute.HPPercentage, (float)CurrentHP / (float)BaseAttributes.MaxHP);
            attributes.Add(Attribute.MPPercentage, BaseAttributes.MaxMP > 0 ? (float)CurrentMP / BaseAttributes.MaxMP : 1.0f);
            attributes.Add(Attribute.StaminaPercentage, (float)CurrentStamina / BaseAttributes.MaxStamina);

            attributes.Add(Attribute.TotalAttack, 
                Mathf.Ceil(attributes[Attribute.Attack] *
                GetHighestBuff(Attribute.Attack) /
                GetHighestDebuff(Attribute.Attack))
            );
            attributes.Add(Attribute.TotalDefense,
                Mathf.Ceil(attributes[Attribute.Defense] *
                GetHighestBuff(Attribute.Defense) /
                GetHighestDebuff(Attribute.Defense))
            );
            attributes.Add(Attribute.TotalMagic, 
                Mathf.Ceil(attributes[Attribute.Magic] *
                GetHighestBuff(Attribute.Magic) /
                GetHighestDebuff(Attribute.Magic))
            );
            attributes.Add(Attribute.TotalResistance,
                Mathf.Ceil(attributes[Attribute.Resistance] *
                GetHighestBuff(Attribute.Resistance) /
                GetHighestDebuff(Attribute.Resistance))
            );
            attributes.Add(Attribute.TotalSpeed,
                Mathf.Ceil(attributes[Attribute.Speed] *
                GetHighestBuff(Attribute.Speed) /
                GetHighestDebuff(Attribute.Speed))
            );
            attributes.Add(Attribute.TotalHit,
                attributes[Attribute.Hit] * 
                GetHighestBuff(Attribute.Hit)
            );
            return attributes;
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
