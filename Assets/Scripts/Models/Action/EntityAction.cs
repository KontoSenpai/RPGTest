using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Managers;
using RPGTest.Models.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.Models.Action
{
    public enum ActionState
    {
        Pending,
        Ready,
        Casting,
        Executing,
        Completed,
        Interupted,
        Cancelled
    }

    [Serializable]
    public partial class EntityAction
    {
        public ActionState ActionState { get; private set; }

        public ActionType ActionType { get; set; } = ActionType.Ability;

        public string ActionId { get; set; }

        public Entity.Entity Caster { get; }
        public List<Entity.Entity> Targets { get; set; } = new List<Entity.Entity>();

        public float CastTime { get; private set; }
        private float m_delay = 1.0f;
        private float m_endDelay = 0.75f;

        public event ActionMessageHandler ActionLogged;
        public delegate void ActionMessageHandler(string display);

        public event ActionExecutionHandler ActionExecuted = delegate {};
        public delegate void ActionExecutionHandler(Entity.Entity target, Enums.EffectType type, Enums.Attribute attribute, int value);

        private InventoryManager m_InventoryManager;

        public EntityAction(Entity.Entity caster, ActionType actionType, string actionId, List<Entity.Entity> targets, InventoryManager manager = null)
        {
            Caster = caster;
            ActionType = actionType;
            ActionId = actionId;
            Targets = targets;
            m_InventoryManager = manager;

            switch (ActionType)
            {
                case ActionType.Ability:
                    var ability = AbilitiesCollector.TryGetAbility(ActionId);
                    CastTime = ability.CastTime;
                    break;
                case ActionType.Item:
                    CastTime = 0.5f;
                    break;
                default:
                    CastTime = 1.0f;
                    break;
            }
        }

        /// <summary>
        /// Retrieve action name for display.
        /// </summary>
        /// <returns>Action name</returns>
        public string GetActionName()
        {
            switch(ActionType)
            {
                case ActionType.Ability:
                case ActionType.AbilityMenu:
                    return AbilitiesCollector.TryGetAbility(ActionId).Name;
                case ActionType.Item:
                case ActionType.ItemMenu:
                    return ItemCollector.TryGetItem(ActionId).Name;
                default:
                    return string.Empty;
            }
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
        private List<Entity.Entity> GetTargets(TargetType targetType, List<PlayableCharacter> allies = null, List<Enemy> enemies = null)
        {
            var targets = new List<Entity.Entity>();
            switch (targetType)
            {
                case TargetType.None:
                    targets.AddRange(Targets);
                    break;
                case TargetType.Enemies:
                    targets.AddRange(enemies);
                    break;
                case TargetType.Allies:
                    targets.AddRange(allies);
                    break;
                case TargetType.Self:
                    targets.Add(Caster);
                    break;
                case TargetType.All:
                    targets.AddRange(allies);
                    targets.AddRange(enemies);
                    break;
            }
            return targets;
        }

        public IEnumerator Execute(List<PlayableCharacter> allies, List<Enemy> enemies)
        {
            ActionState = ActionState.Executing;

            List<Effect> effects = new List<Effect>();
            switch (ActionType)
            {
                case ActionType.Ability:
                    var ability = AbilitiesCollector.TryGetAbility(ActionId);

                    if(!Caster.IsAbilityCastable(ability))
                    {
                        ActionState = ActionState.Completed;
                        ActionLogged($"{Caster.Name} tries to use {GetActionName()} but failed to! (missing resources)");
                        yield break;
                    }
                    yield return new WaitForSeconds(m_delay);
                    if(ActionState != ActionState.Cancelled)
                    {
                        foreach (var resourceCost in ability.CastCost)
                        {
                            Caster.ApplyAttributeModification(resourceCost.Key, -resourceCost.Value);
                        }
                        effects = ability.Effects;
                    }
                    ActionLogged($"{Caster.Name} uses {GetActionName()} on {string.Join(", ", Targets.Select(x => x.Name))}");
                    break;
                case ActionType.Item:
                    effects = ItemCollector.TryGetConsumable(ActionId).Effects;
                    if (m_InventoryManager != null)
                    {
                        m_InventoryManager.RemoveItem(ActionId, 1);
                    }
                    ActionLogged($"{Caster.Name} uses {GetActionName()} on {string.Join(", ", Targets.Select(x => x.Name))}");
                    break;
                case ActionType.ItemMenu:
                    effects = ItemCollector.TryGetConsumable(ActionId).Effects;
                    if(m_InventoryManager != null)
                    {
                        m_InventoryManager.RemoveItem(ActionId, 1);
                    }
                    break;
            }

            foreach (var effect in effects)
            {
                switch (effect.EffectType)
                {
                    case EffectType.Damage:
                        ExecuteDamage(effect, allies, enemies);
                        break;
                    case EffectType.Heal:
                        ExecuteHeal(effect, allies, enemies);
                        break;
                    case EffectType.Buff:
                        ExecuteBuff(effect, allies, enemies);
                        break;
                    default:
                        Debug.LogError("effect type not supported : ");
                        break;
                }
            }

            yield return new WaitForSeconds(m_endDelay);
            ActionState = ActionState.Completed;  
        }


        public void InterruptCast()
        {
            ActionState = ActionState.Interupted;
        }

        public void CancelCast()
        {
            ActionState = ActionState.Cancelled;
        }
    }
}
