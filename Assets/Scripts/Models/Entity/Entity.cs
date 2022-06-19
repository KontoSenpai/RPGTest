using RPGTest.Enums;
using RPGTest.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

namespace RPGTest.Models.Entity
{
    public enum ActionType
    {
        Ability,
        Item,
        AbilityMenu,
        ItemMenu,
        Equip
    }

    public abstract class Entity : IdObject
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

        public Dictionary<StatusEffect, int> StatusEffects { get; set; }

        public Dictionary<StatusEffect, int> StatusEffectResistance { get; set; }

        public Dictionary<Element, int> ElementResistance { get; set; }

        public virtual Range PowerRange { get; set; } = new Range() { Min = 0.75f, Max = 1.25f };
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
            return m_currentATB > m_maxATB;
        }

        public abstract IEnumerator SelectAction(BattleManager manager, List<PlayableCharacter> playerParty, List<Enemy> enemyParty, System.Action<ActionSequence> selectedAction);

        public virtual void ResetATB(int variation = 0)
        {
            m_currentATB = 0;
            m_maxATB = m_baseMaxATB + variation;
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

        public virtual void ApplyResourceModification(Attribute attribute, int value)
        {
            switch (attribute)
            {
                case Attribute.MaxHP:
                case Attribute.HP:
                    CurrentHP += value;
                    if(CurrentHP < 0)
                    {
                        CurrentHP = 0;
                    }
                    else if(CurrentHP > BaseAttributes.MaxHP)
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

        public bool InflictStatusEffect(StatusEffect effect, int potency)
        {
            return false;
        }

        public virtual float GetAttribute(Attribute attribute)
        {
            GetAttributes().TryGetValue(attribute, out float value);
            return value;
        }

        public virtual Dictionary<Attribute, float> GetAttributes()
        {
            Dictionary<Attribute, float> attributes = new Dictionary<Attribute, float>();

            attributes.Add(Attribute.HP, CurrentHP);
            attributes.Add(Attribute.HPPercentage, (float)CurrentHP / (float)BaseAttributes.MaxHP);
            attributes.Add(Attribute.MaxHP, BaseAttributes.MaxHP);

            attributes.Add(Attribute.MP, CurrentMP);
            attributes.Add(Attribute.MPPercentage, BaseAttributes.MaxMP > 0 ? (float)CurrentMP / BaseAttributes.MaxMP : 1.0f);
            attributes.Add(Attribute.MaxMP, BaseAttributes.MaxMP);

            attributes.Add(Attribute.Stamina, CurrentStamina);
            attributes.Add(Attribute.StaminaPercentage, (float)CurrentStamina / BaseAttributes.MaxStamina);
            attributes.Add(Attribute.MaxStamina, BaseAttributes.MaxStamina);

            attributes.Add(Attribute.Attack, BaseAttributes.Attack);
            attributes.Add(Attribute.Defense, BaseAttributes.Defense);

            attributes.Add(Attribute.Magic, BaseAttributes.Magic);
            attributes.Add(Attribute.Resistance, BaseAttributes.Resistance);

            attributes.Add(Attribute.Speed, BaseAttributes.Speed);

            return attributes;
        }
    }
}
