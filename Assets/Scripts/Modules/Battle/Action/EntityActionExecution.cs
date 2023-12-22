using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Effects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.Modules.Battle.Action
{
    public partial class EntityAction
    {
        Dictionary<int, List<EffectEvaluation>> m_effectEvaluations;

        /// <summary>
        /// Execute and apply any changes evaluated in the previous step, sequencially for all targets
        /// </summary>
        /// <returns></returns>
        public IEnumerator RegisterEffects(MonoBehaviour managerBehaviour)
        {
            foreach(var effectEvaluations in m_effectEvaluations)
            {
                foreach(var effectEvaluation in effectEvaluations.Value)
                {
                    managerBehaviour.StartCoroutine(ExecuteEffectEvaluation(effectEvaluation));
                }
                while (effectEvaluations.Value.Any(a => a.State == ActionState.Executing))
                {
                    yield return null;
                }
            }
            yield return new WaitForSeconds(m_endDelay);
        }

        private IEnumerator ExecuteEffectEvaluation(EffectEvaluation effectEvaluation)
        {
            if (effectEvaluation.Frames.Count != effectEvaluation.HitPower.Count)
            {
                Debug.LogError("Mismatch between frame counts and ");
                yield break;
            }
            effectEvaluation.State = ActionState.Executing;
            for (int i = 0; i < effectEvaluation.Frames.Count; i++)
            {
                ExecuteEffect(effectEvaluation, effectEvaluation.HitPower[i]);
                yield return new WaitForSeconds(effectEvaluation.Frames[i]);
            }
            effectEvaluation.State = ActionState.Completed;
            yield return null;
        }

        private void ExecuteEffect(EffectEvaluation effect, float coef)
        {
            switch (effect.EffectType)
            {
                case EffectType.Damage:
                    ExecuteDamage(effect, coef);
                    break;
                case EffectType.Heal:
                    ExecuteHeal(effect, coef);
                    break;
                case EffectType.Buff:
                case EffectType.Debuff:
                    ExecuteAttributeChange(effect, coef);
                    break;
                case EffectType.Cleanse:
                    ExecuteCleanse(effect);
                    break;
                default:
                    Debug.LogError("effect type not supported : ");
                    break;
            }
        }

        /// <summary>
        /// Execute the process of registering a damage application on the appropriate targets
        /// </summary>
        /// <param name="effect">Effect to be applied</param>
        /// <param name="allies">List of allies of the caster</param>
        /// <param name="enemies">List of enemies of the caster</param>
        private void ExecuteDamage(EffectEvaluation effect, float coefficent)
        {
            var framePower = Mathf.CeilToInt(effect.Value * coefficent);
            effect.Target.ApplyAttributeModification(effect.Attribute, framePower);
            EffectApplied(effect, framePower);
        }

        /// <summary>
        /// Execute the process of registering a heal application on the appropriate targets
        /// </summary>
        /// <param name="effect">Effect to be applied</param>
        /// <param name="allies">List of allies of the caster</param>
        /// <param name="enemies">List of enemies of the caster</param>
        public void ExecuteHeal(EffectEvaluation effect, float coefficent)
        {
            var framePower = Mathf.CeilToInt(effect.Value * coefficent);
            effect.Target.ApplyAttributeModification(effect.Attribute, framePower);
            EffectApplied(effect, framePower);
        }

        private void ExecuteAttributeChange(EffectEvaluation effect, float coefficent)
        {            
            effect.Target.EffectsComponent.AddEffect(new Effect()
            {
                Id = effect.Id,
                Duration = effect.Duration,
            });

            EffectApplied(effect, 0);
        }

        private void ExecuteCleanse(EffectEvaluation effect)
        {
            effect.Target.EffectsComponent.RemoveEffects(effect.RemovalType, effect.Attribute);
            effect.Target.RemoveStatusEffect(effect.StatusEffect, effect.RemovalType);
            EffectApplied(effect, 0);
        }
    }
}
