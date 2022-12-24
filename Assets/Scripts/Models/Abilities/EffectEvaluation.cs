using RPGTest.Enums;
using RPGTest.Models.Entity;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.Models.Abilities
{
    public partial class Effect
    {
        private ActionType m_actionType;
        private List<Entity.Entity> m_targets;

        public List<EffectEvaluation> Evaluate(
            ActionType type,
            Entity.Entity caster, 
            List<Entity.Entity> targets,
            List<PlayableCharacter> allies,
            List<Enemy> enemies
            )
        {
            m_actionType = type;
            m_targets = targets;
            switch (Type)
            {
                case EffectType.Damage:
                    return EvaluateDamage(caster, allies, enemies);
                case EffectType.Heal:
                    return EvaluateHeal(caster, allies, enemies);
                case EffectType.Buff:
                    return EvaluateBuff(caster, allies, enemies);
                case EffectType.Debuff:
                    return EvaluateDebuff(caster, allies, enemies);
                default:
                    Debug.LogError("effect type not supported : ");
                    return null;
            }
        }

        /// <summary>
        /// Execute the process of registering a damage application on the appropriate targets
        /// </summary>
        /// <param name="effect">Effect to be applied</param>
        /// <param name="allies">List of allies of the caster</param>
        /// <param name="enemies">List of enemies of the caster</param>
        public List<EffectEvaluation> EvaluateDamage(
            Entity.Entity caster,
            List<PlayableCharacter> allies,
            List<Enemy> enemies
        )
        {
            var effectEvaluations = new List<EffectEvaluation>();
            foreach (var target in GetTargets(TargetType, caster, allies, enemies))
            {
                var value = target.CalculateDamage(caster, this);

                effectEvaluations.Add(new EffectEvaluation
                {
                    ActionType = m_actionType,
                    EffectType = Type,
                    Target = target,
                    Attribute = Potency.Attribute,
                    Value = value,
                });

                //target.ApplyAttributeModification(attribute, value);
            }
            return effectEvaluations;
        }

        /// <summary>
        /// Execute the process of registering a heal application on the appropriate targets
        /// </summary>
        /// <param name="effect">Effect to be applied</param>
        /// <param name="allies">List of allies of the caster</param>
        /// <param name="enemies">List of enemies of the caster</param>
        public List<EffectEvaluation> EvaluateHeal(
            Entity.Entity caster,
            List<PlayableCharacter> allies,
            List<Enemy> enemies
        )
        {
            var effectEvaluations = new List<EffectEvaluation>();
            foreach (var target in GetTargets(TargetType, caster, allies, enemies))
            {
                var value = target.CalculateHealing(caster, this);

                effectEvaluations.Add(new EffectEvaluation
                {
                    ActionType = m_actionType,
                    EffectType = Type,
                    Target = target,
                    Attribute = Potency.Attribute,
                    Value = value,
                });

                //target.ApplyAttributeModification(attribute, value);
            }
            return effectEvaluations;
        }

        public List<EffectEvaluation> EvaluateBuff(
            Entity.Entity caster,
            List<PlayableCharacter> allies,
            List<Enemy> enemies
        )
        {
            return EvaluateBuffDebuff(GetTargets(TargetType, caster, allies, enemies), this, false);
        }

        public List<EffectEvaluation> EvaluateDebuff(
            Entity.Entity caster,
            List<PlayableCharacter> allies,
            List<Enemy> enemies
        )
        {
            return EvaluateBuffDebuff(GetTargets(TargetType, caster, allies, enemies), this, true);
        }

        private List<EffectEvaluation> EvaluateBuffDebuff(
            List<Entity.Entity> targets,
            Effect effect,
            bool debuff
        )
        {
            var effectEvaluations = new List<EffectEvaluation>();
            foreach (var target in targets)
            {
                var value = Mathf.CeilToInt(effect.Potency.Potency / 100);
                //Buff buffInstance = new Buff
                //{
                //    Attribute = attribute,
                //    Value = debuff ? potency * -1 : potency,
                //    Duration = effect.Potency.Duration,
                //    RemovalType = effect.Potency.RemovalType
                //};

                effectEvaluations.Add(new EffectEvaluation
                {
                    ActionType = m_actionType,
                    EffectType = Type,
                    Target = target,
                    Attribute = Potency.Attribute,
                    Value = debuff ? value * -1 : value,
                    Duration = Potency.Duration,
                    Frames = HitFrames,
                    HitPower = HitPower,
                    RemovalType = Potency.RemovalType,
                });


                //target.AddBuff(buffInstance);
            }
            return effectEvaluations;
        }

        /// <summary>
        /// Used to retrieve target for action effects.
        /// Mainly for actions with multiple effects.
        /// Ie attack a single target, but inflict poison on the whole party
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="allies">Allies present on the battlefield</param>
        /// <param name="enemies">Enemis present on the battlefield</param>
        /// <returns>Desired targets</returns>
        private List<Entity.Entity> GetTargets(TargetType targetType, Entity.Entity caster, List<PlayableCharacter> allies = null, List<Enemy> enemies = null)
        {
            var targets = new List<Entity.Entity>();
            switch (targetType)
            {
                case TargetType.None:
                    targets.AddRange(m_targets);
                    break;
                case TargetType.Enemies:
                    targets.AddRange(enemies);
                    break;
                case TargetType.Allies:
                    targets.AddRange(allies);
                    break;
                case TargetType.Self:
                    targets.Add(caster);
                    break;
                case TargetType.All:
                    targets.AddRange(allies);
                    targets.AddRange(enemies);
                    break;
            }
            return targets;
        }
    }
}
