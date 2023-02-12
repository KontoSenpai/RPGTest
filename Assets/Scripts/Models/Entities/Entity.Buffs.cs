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
        public virtual float GetHighestBuff(Attribute attribute)
        {
            return GetHighestStatChange(attribute, EffectType.Buff);
        }

        public virtual float GetHighestDebuff(Attribute attribute)
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
            OnBuffsRefreshed(new BuffsRefreshedArgs(Buffs));
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
                    Debug.Log(effect.Id);
                    return effect.Potency.Attribute == attribute && b.RemovalType == removalType;
                });
            }
            OnBuffsRefreshed(new BuffsRefreshedArgs(Buffs));
        }

        public virtual void RemoveBuff(Buff buff)
        {
            Buffs.Remove(buff);
            OnBuffsRefreshed(new BuffsRefreshedArgs(Buffs));
        }
    }
}
