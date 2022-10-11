using RPGTest.Models.Entity;
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
            foreach (var attribute in effect.Attributes)
            {
                List<Entity.Entity> targets = new List<Entity.Entity>();
                float attackPower = attribute.Value.Potency;
                foreach (var scaling in effect.Scalings)
                {
                    attackPower += Caster.GetAttribute(scaling.Key) * scaling.Value;
                }

                foreach (var target in GetTargets(effect.TargetType, allies, enemies))
                {
                    var attackValue = (int)Mathf.Ceil((attackPower * -1) * effect.PowerRange.GetValue());

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
                var buffPotency = (int)Mathf.Ceil(attribute.Value.Potency);

                foreach (var target in GetTargets(effect.TargetType, allies, enemies))
                {
                    target.ApplyBuff(attribute.Key, buffPotency, attribute.Value.Duration, attribute.Value.RemovalType);

                    if (ActionType == ActionType.Ability || ActionType == ActionType.Item)
                    {
                        ActionExecuted(target, effect.EffectType, attribute.Key, buffPotency);
                    }
                }
            }
        }
    }
}
