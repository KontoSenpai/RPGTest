using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Models.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using Attribute = RPGTest.Enums.Attribute;

namespace RPGTest.Models.Entity.Components
{
    /// <summary>
    /// EffectsComponent is a component to hold and handle all <see cref="Effect"/> applied to an <see cref="Entity"/>
    /// </summary>
    public class EffectsComponent
    {
        public event EventHandler<BuffsRefreshedArgs> BuffsRefreshed;

        private Dictionary<string, int> m_effects { get; set; } = new Dictionary<string, int>();

        #region Event handling
        protected virtual void OnBuffsRefreshed(BuffsRefreshedArgs e)
        {
            BuffsRefreshed?.Invoke(this, e);
        }
        #endregion

        #region Getters
        /// <summary>
        /// Returns every active effects applied to an <see cref="Entity"/>
        /// </summary>
        /// <returns>A list of active <see cref="Effect"/></returns>
        public IEnumerable<Effect> GetEffects()
        {
            return EffectsCollector.TryGetEffects(m_effects.Keys);
        }

        public IEnumerable<Effect> GetBuffs()
        {
            return GetEffectsInternal(EffectType.Buff);
        }

        public IEnumerable<Effect> GetDebuffs()
        {
            return GetEffectsInternal(EffectType.Debuff);
        }

        public IEnumerable<Effect> GetStatusEffects()
        {
            return GetEffectsInternal(EffectType.Status);
        }
        #endregion

        /// <summary>
        /// Add or refresh an effect to an <see cref="Entity"/>
        /// </summary>
        /// <param name="effect"><see cref="Effect"/> to add or refresh</param>
        public void AddEffect(Effect effect)
        {
            if (m_effects.ContainsKey(effect.Id))
            {
                m_effects[effect.Id] = effect.Duration;
            }
            else
            {
                m_effects.Add(effect.Id, effect.Duration);
            }

            OnBuffsRefreshed(new BuffsRefreshedArgs(GetBuffs(), GetDebuffs()));
        }

        public virtual void RemoveEffectById(string effectId)
        {
            if (m_effects.ContainsKey(effectId))
            {
                m_effects.Remove(effectId);

                OnBuffsRefreshed(new BuffsRefreshedArgs(GetBuffs(), GetBuffs()));
            }
        }

        /// <summary>
        /// Remove effects corresponding to the given removal type rule
        /// </summary>
        /// <param name="removalType">RemovalType of the action</param>
        public virtual void RemoveEffects(RemovalType removalType, Attribute attribute = Attribute.None)
        {
            var effects = EffectsCollector.TryGetEffects(m_effects.Keys);
            var effectsToRemove = effects.Where(e => {
                if (e.RemovalType != removalType)
                {
                    return false;
                }

                if (attribute != Attribute.None && e.Potency.Attribute != attribute)
                {
                    return false;
                }

                return true;
            });


            foreach(var effect in effectsToRemove)
            {
                m_effects.Remove(effect.Id);
            }

            OnBuffsRefreshed(new BuffsRefreshedArgs(GetBuffs(), GetBuffs()));
        }

        public void TickDownEffects()
        {
            foreach (var effect in m_effects.Keys)
            {
                m_effects[effect] = m_effects[effect]--;
            }

            foreach(var e in m_effects.Where(e => e.Value <= 0).ToList())
            {
                m_effects.Remove(e.Key);
            }

            OnBuffsRefreshed(new BuffsRefreshedArgs(GetBuffs(), GetDebuffs()));
        }

        #region Private Methods
        private IEnumerable<Effect> GetEffectsInternal(EffectType type)
        {
            List<Effect> effects = new List<Effect>();
            foreach (var effect in m_effects)
            {
                var e = EffectsCollector.TryGetEffect(effect.Key);
                if (e != null && e.Type == type)
                {
                    effects.Add(e);
                }
            }
            return effects;
        }
        #endregion


    }
}