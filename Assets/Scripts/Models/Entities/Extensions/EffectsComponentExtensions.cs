using System.Linq;
using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Models.Entity.Components;


namespace RPGTest.Models.Entity.Extensions
{
    public static class EffectsComponentExtensions
    {
        /// <summary>
        /// Retrieves the Buff of the highest value for given <see cref="Attribute"/>
        /// </summary>
        /// <param name="component"><see cref="EffectsComponent"/> of the <see cref="Entity"/> to calculate the Buff of</param>
        /// <param name="attribute"><see cref="Attribute"/> to get the buff of</param>
        /// <returns>The value of the Attribute Buff. 1.0f if there isn't a Buff for given Attribute</returns>
        public static float GetHighestAttributeBuff(this EffectsComponent component, Attribute attribute)
        {
            return component.GetHighestAttributeChange(EffectType.Buff, attribute);
        }

        /// <summary>
        /// Retrieves the Debuff of the highest value for given <see cref="Attribute"/>
        /// </summary>
        /// <param name="component"><see cref="EffectsComponent"/> of the <see cref="Entity"/> to calculate the Debuff of</param>
        /// <param name="attribute"><see cref="Attribute"/> to get the Debuff of</param>
        /// <returns>The value of the Attribute Debuff. 1.0f if there isn't a Debuff for given Attribute</returns>
        public static float GetHighestAttributeDebuff(this EffectsComponent component, Attribute attribute)
        {
            return component.GetHighestAttributeChange(EffectType.Debuff, attribute);
        }

        /// <summary>
        /// Retrieves the Buff of the highest value for given <see cref="Element"/>
        /// </summary>
        /// <param name="component"><see cref="EffectsComponent"/> of the <see cref="Entity"/> to calculate the Buff of</param>
        /// <param name="element"><see cref="Element"/> to get the buff of</param>
        /// <returns>The value of the Attribute Buff. 1.0f if there isn't a Buff for given Attribute</returns>
        public static float GetHighestElementalResistanceBuff(this EffectsComponent component, Element element)
        {
            return component.GetHighestElementalResistanceChange(element, EffectType.Buff);
        }

        /// <summary>
        /// Retrieves the Debuff of the highest value for given <see cref="Element"/>
        /// </summary>
        /// <param name="component"><see cref="EffectsComponent"/> of the <see cref="Entity"/> to calculate the Debuff of</param>
        /// <param name="element"><see cref="Element"/> to get the Debuff of</param>
        /// <returns>The value of the Attribute Debuff. 1.0f if there isn't a Debuff for given Attribute</returns>
        public static float GetHighestElementalResistanceDebuff(this EffectsComponent component, Element element)
        {
            return component.GetHighestElementalResistanceChange(element, EffectType.Debuff);
        }

        public static float GetElementResistanceVariation(this EffectsComponent component, Element element)
        {
            return component.GetHighestElementalResistanceBuff(element) - component.GetHighestElementalResistanceDebuff(element);
        }

        /// <summary>
        /// Retrieves the Buff of the highest value for given <see cref="StatusEffect"/>
        /// </summary>
        /// <param name="component"><see cref="EffectsComponent"/> of the <see cref="Entity"/> to calculate the Buff of</param>
        /// <param name="element"><see cref="Element"/> to get the buff of</param>
        /// <returns>The value of the Attribute Buff. 1.0f if there isn't a Buff for given Attribute</returns>
        public static float GetHighestStatusEffectResistanceBuff(this EffectsComponent component, StatusEffect statusEffect)
        {
            return component.GetHighestStatusEffectResistanceChange(statusEffect, EffectType.Buff);
        }

        /// <summary>
        /// Retrieves the Buff of the highest value for given <see cref="StatusEffect"/>
        /// </summary>
        /// <param name="component"><see cref="EffectsComponent"/> of the <see cref="Entity"/> to calculate the Buff of</param>
        /// <param name="element"><see cref="Element"/> to get the buff of</param>
        /// <returns>The value of the Attribute Buff. 1.0f if there isn't a Buff for given Attribute</returns>
        public static float GetHighestStatusEffectResistanceDebuff(this EffectsComponent component, StatusEffect statusEffect)
        {
            return component.GetHighestStatusEffectResistanceChange(statusEffect, EffectType.Debuff);
        }

        public static float GetStatusEffectResistanceVariation(this EffectsComponent component, StatusEffect statusEffect)
        {
            return component.GetHighestStatusEffectResistanceBuff(statusEffect) - component.GetHighestStatusEffectResistanceDebuff(statusEffect);
        }

        #region Private Methods
        private static float GetHighestAttributeChange(this EffectsComponent component, EffectType type, Attribute attribute)
        {
            var effects = EffectsCollector.TryGetEffects(component.GetEffects().Select(x => x.Id).ToList());
            var change = effects.FindAll(e => e.Potency.Attribute == attribute && e.Type == type)
                .OrderByDescending(e => e.Potency.Potency)
                .FirstOrDefault();

            if (change != null)
            {
                return 1 + change.Potency.Potency;
            }
            return 1.0f;
        }

        private static float GetHighestElementalResistanceChange(this EffectsComponent component, Element element, EffectType type)
        {
            var effects = EffectsCollector.TryGetEffects(component.GetEffects().Select((b) => b.Id).ToList());
            var change = effects.FindAll(e => e.Potency.Element == element && e.Type == type)
                .OrderByDescending(e => e.Potency.Potency)
                .FirstOrDefault();

            if (change != null)
            {
                return change.Potency.Potency;
            }

            return 0.0f;
        }

        private static float GetHighestStatusEffectResistanceChange(this EffectsComponent component, StatusEffect statusEffect, EffectType type)
        {
            var effects = EffectsCollector.TryGetEffects(component.GetEffects().Select((b) => b.Id).ToList());
            var change = effects.FindAll(e => e.Potency.StatusEffect == statusEffect && e.Type == type)
                .OrderByDescending(e => e.Potency.Potency)
                .FirstOrDefault();

            if (change != null)
            {
                return change.Potency.Potency;
            }

            return 0.0f;
        }
        #endregion

    }
}