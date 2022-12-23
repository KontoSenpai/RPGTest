using RPGTest.Models;
using RPGTest.Models.Entity;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.Modules.Battle.Action
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
            var attribute = effect.Potency.Attribute;
            foreach (var target in GetTargets(effect.TargetType, allies, enemies))
            {
                var attackValue = target.CalculateDamage(Caster, effect, effect.Potency);

                target.ApplyAttributeModification(attribute, attackValue);

                ActionExecuted(target, effect.Type, attribute, attackValue);
            }
        }

        /// <summary>
        /// Execute the process of registering a heal application on the appropriate targets
        /// </summary>
        /// <param name="effect">Effect to be applied</param>
        /// <param name="allies">List of allies of the caster</param>
        /// <param name="enemies">List of enemies of the caster</param>
        public void ExecuteHeal(Effect effect, List<PlayableCharacter> allies, List<Enemy> enemies)
        {
            var attribute = effect.Potency.Attribute;
            foreach (var target in GetTargets(effect.TargetType, allies, enemies))
            {
                var healValue = target.CalculateHealing(Caster, effect, effect.Potency);

                target.ApplyAttributeModification(attribute, healValue);

                ActionExecuted(target, effect.Type, attribute, healValue);
            }
        }

        public void ExecuteBuff(Effect effect, List<PlayableCharacter> allies, List<Enemy> enemies)
        {
            ExecuteBuffDebuff(GetTargets(effect.TargetType, allies, enemies), effect, false);
        }

        public void ExecuteDebuff(Effect effect, List<PlayableCharacter> allies, List<Enemy> enemies)
        {
            ExecuteBuffDebuff(GetTargets(effect.TargetType, allies, enemies), effect, true);
        }

        private void ExecuteBuffDebuff(List<Entity> targets, Effect effect, bool debuff)
        {
            var attribute = effect.Potency.Attribute;
            foreach (var target in targets)
            {
                var potency = (effect.Potency.Potency / 100);
                Buff buffInstance = new Buff
                {
                    Attribute = attribute,
                    Value = debuff? potency * -1 : potency,
                    Duration = effect.Potency.Duration,
                    RemovalType = effect.Potency.RemovalType
                };

                target.AddBuff(buffInstance);

                if (ActionType == ActionType.Ability || ActionType == ActionType.Item)
                {
                    ActionExecuted(target, effect.Type, attribute, (int)(buffInstance.Value * 100));
                }
            }
        }
    }
}
