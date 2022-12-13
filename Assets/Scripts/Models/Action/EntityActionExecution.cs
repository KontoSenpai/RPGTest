﻿using RPGTest.Models.Entity;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.Models.Action
{
    public partial class EntityAction
    {
        /// <summary>
        /// Execute the process of registering a damage application on the appropriate targets
        /// </summary>
        /// <param name="effect">Effect to be applied</param>
        /// <param name="allies">List of allies of the caster</param>
        /// <param name="enemies">List of enemies of the caster</param>
        public void ExecuteDamage(Effect effect, List<PlayableCharacter> allies, List<Enemy> enemies)
        {
            foreach (var target in GetTargets(effect.TargetType, allies, enemies))
            {
                foreach (var attribute in effect.Attributes)
                {
                    var attackValue = target.CalculateDamage(Caster, effect, attribute.Value);

                    target.ApplyAttributeModification(attribute.Key, attackValue);

                    ActionExecuted(target, effect.EffectType, attribute.Key, attackValue);
                }
            }
        }

        public void ExecuteHeal(Effect effect, List<PlayableCharacter> allies, List<Enemy> enemies)
        {
            foreach (var attribute in effect.Attributes)
            {
                List<Entity.Entity> targets = new List<Entity.Entity>();
                var healPotency = attribute.Value.Potency;
                foreach (var scaling in effect.Scalings)
                {
                    healPotency += Caster.GetAttribute(scaling.Key) * scaling.Value;
                }

                foreach (var target in GetTargets(effect.TargetType, allies, enemies))
                {
                    var healValue = (int)Mathf.Ceil(healPotency * effect.PowerRange.GetValue());

                    target.ApplyAttributeModification(attribute.Key, healValue);
                    if (ActionType == ActionType.Ability || ActionType == ActionType.Item)
                    {
                        ActionExecuted(target, effect.EffectType, attribute.Key, healValue);
                    }
                }
            }
        }

        public void ExecuteBuff(Effect effect, List<PlayableCharacter> allies, List<Enemy> enemies)
        {
            foreach (var attribute in effect.Attributes)
            {
                List<Entity.Entity> targets = new List<Entity.Entity>();

                Buff buff = new Buff
                {
                    Attribute = attribute.Key,
                    Value = attribute.Value.Potency / 100,
                    Duration = attribute.Value.Duration,
                    RemovalType = attribute.Value.RemovalType
                };

                foreach (var target in GetTargets(effect.TargetType, allies, enemies))
                {
                    target.AddBuff(buff);

                    if (ActionType == ActionType.Ability || ActionType == ActionType.Item)
                    {
                        ActionExecuted(target, effect.EffectType, attribute.Key, (int)(buff.Value * 100));
                    }
                }
            }
        }
    }
}