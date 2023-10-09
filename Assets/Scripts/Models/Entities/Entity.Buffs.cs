using RPGTest.Collectors;
using RPGTest.Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Attribute = RPGTest.Enums.Attribute;

namespace RPGTest.Models.Entity
{
    public abstract partial class Entity : IdObject
    {
        public virtual float GetHighestAttributeBuff(Attribute attribute)
        {
            return GetHighestStatChange(attribute, EffectType.Buff);
        }

        public virtual float GetHighestAttributeDebuff(Attribute attribute)
        {
            return GetHighestStatChange(attribute, EffectType.Debuff);
        }

        private float GetHighestStatChange(Attribute attribute, EffectType type)
        {
            var effects = EffectsCollector.TryGetEffects(Buffs.Select(x => x.Id).ToList());
            var change = effects.FindAll(e => e.Potency.Attribute == attribute && e.Type == type)
                .OrderByDescending(e => e.Potency.Potency)
                .FirstOrDefault();

            if (change != null)
            {
                return 1 + change.Potency.Potency;
            }
            return 1.0f;
        }

        public virtual float GetHighestElementalResistanceBuff(Element element)
        {
            return GetHighestElementalResistanceChange(element, EffectType.Buff);
        }

        public virtual float GetHighestElementalResistanceDebuff(Element element)
        {
            return GetHighestElementalResistanceChange(element, EffectType.Debuff);
        }

        private float GetHighestElementalResistanceChange(Element element, EffectType type)
        {
            var effects = EffectsCollector.TryGetEffects(Buffs.Select((b) => b.Id).ToList());
            var change = effects.FindAll(e => e.Potency.Element == element && e.Type == type)
                .OrderByDescending(e => e.Potency.Potency)
                .FirstOrDefault();

            if (change != null)
            {
                return change.Potency.Potency;
            }

            return 0.0f;
        }

        public virtual float GetHighestStatusEffectResistanceBuff(StatusEffect statusEffect)
        {
            return GetHighestStatusEffectResistanceChange(statusEffect, EffectType.Buff);
        }

        public virtual float GetHighestStatusEffectResistanceDebuff(StatusEffect statusEffect)
        {
            return GetHighestStatusEffectResistanceChange(statusEffect, EffectType.Debuff);
        }

        private float GetHighestStatusEffectResistanceChange(StatusEffect statusEffect, EffectType type)
        {
            var effects = EffectsCollector.TryGetEffects(Buffs.Select((b) => b.Id).ToList());
            var change = effects.FindAll(e => e.Potency.StatusEffect == statusEffect && e.Type == type)
                .OrderByDescending(e => e.Potency.Potency)
                .FirstOrDefault();

            if (change != null)
            {
                return change.Potency.Potency;
            }

            return 0.0f;
        }

        private List<Buff> GetBuffs(EffectType type)
        {
            var effectIds = EffectsCollector.TryGetEffects(Buffs.Select(x => x.Id).ToList())
                .Where(e => e.Type == type)
                .Select(e => e.Id);

            return Buffs.Where(b => effectIds.Contains(b.Id)).ToList();
        }

        /// <summary>
        /// Apply a buff to the selected Entity.
        /// If a a buff of a same value is re-applied, it's duration will be extended
        /// </summary>
        /// <param name="entity">Entity on which the modification should be applied to</param>
        /// <param name="buff">Buff to assign</param>
        public virtual void AddBuff(Buff buff)
        {
            int existingBuffIndex = Buffs.FindIndex(b => b.Id == buff.Id);

            if (existingBuffIndex != -1)
            {
                Buffs[existingBuffIndex] = buff;
            }
            else
            {
                Buffs.Add(buff);
            }
            OnBuffsRefreshed(new BuffsRefreshedArgs(GetBuffs(EffectType.Buff), GetBuffs(EffectType.Debuff)));
        }

        /// <summary>
        /// Remove buffs corresponding to the given removal type rule
        /// </summary>
        /// <param name="removalType">RemovalType of the action</param>
        public virtual void RemoveBuffs(Attribute attribute, RemovalType removalType)
        {
            if (attribute == Attribute.None)
            {
                Buffs.RemoveAll(b => b.RemovalType == removalType);
            } 
            else
            {
                Buffs.RemoveAll(b => 
                {
                    var effect = EffectsCollector.TryGetEffect(b.Id);
                    return effect.Potency.Attribute == attribute && b.RemovalType == removalType;
                });
            }


            OnBuffsRefreshed(new BuffsRefreshedArgs(GetBuffs(EffectType.Buff), GetBuffs(EffectType.Debuff)));
        }

        public virtual void RemoveBuff(Buff buff)
        {
            Buffs.Remove(buff);
            OnBuffsRefreshed(new BuffsRefreshedArgs(GetBuffs(EffectType.Buff), GetBuffs(EffectType.Debuff)));
        }
    }
}
