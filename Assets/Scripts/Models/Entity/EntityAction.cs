using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.Models.Entity
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
    public class ActionSequence
    {
        private ActionState stateBeforeInterruption;
        public ActionState SequenceState { get; private set; }
        public Entity Caster { get; }
        public List<EntityAction> Actions { get; set; } = new List<EntityAction>();

        public float BackSwing { get; set; } = 0;

        private float m_currentCastTime;
        private float m_castTime;
        private float m_timeStep = 0.1f;

        private EntityAction currentAction = null;

        public event ActionSequenceCompletedHandler ActionSequenceCompleted;
        public delegate void ActionSequenceCompletedHandler(ActionSequence actionSequence);

        public ActionSequence(Entity caster)
        {
            Caster = caster;
            SequenceState = ActionState.Pending;
        }

        public void AddAction(EntityAction action)
        {
            Actions.Add(action);
        }

        public IEnumerator BeginSequence()
        {
            SequenceState = ActionState.Casting;

            if (Actions.Count > 1)
            {
                m_castTime = Actions.Sum(x => x.CastTime) * 0.8f;
            }
            else
            {
                m_castTime = Actions.First().CastTime;
            }
            
            while (m_currentCastTime < m_castTime)
            {
                if (SequenceState == ActionState.Casting)
                {
                    yield return new WaitForSeconds(m_timeStep);
                    m_currentCastTime += m_timeStep;
                }
                else if (SequenceState == ActionState.Interupted)
                {
                    yield return new WaitForSeconds(m_timeStep);
                }
                else if (SequenceState == ActionState.Cancelled)
                {
                    break;
                }
            }

            if (SequenceState == ActionState.Casting)
            {
                SequenceState = ActionState.Ready;
            }
        }

        public void InterruptSequence()
        {
            stateBeforeInterruption = SequenceState;
            SequenceState = ActionState.Interupted;
            if(currentAction != null)
            {
                currentAction.InterruptCast();
            }
        }

        public void ResumeSequence()
        {
            if (SequenceState == ActionState.Interupted)
            {
                SequenceState = stateBeforeInterruption;
            }
        }

        public IEnumerator ExecuteSequence(BattleManager manager, List<PlayableCharacter> allies, List<Enemy> enemies)
        {
            SequenceState = ActionState.Executing;
            foreach(EntityAction action in Actions)
            {
                manager.StartCoroutine(action.Execute(allies, enemies));
                while(action.ActionState == ActionState.Executing)
                {
                    if(SequenceState == ActionState.Cancelled)
                    {
                        SequenceState = ActionState.Cancelled;
                        action.CancelCast();
                    }
                    yield return null;
                }
                if (enemies.TrueForAll(x => !x.IsAlive))
                {
                    Debug.Log("WIN");
                    ActionSequenceCompleted(this);
                    break;
                }
                else if (allies.TrueForAll(x => !x.IsAlive))
                {
                    Debug.Log("LOSE");
                    ActionSequenceCompleted(this);
                    break;
                }
            }

            if(Actions.All(a => a.ActionState == ActionState.Completed))
            {
                SequenceState = ActionState.Completed;
            }

            if(SequenceState == ActionState.Completed)
            {
                //ActionSequenceCompleted(this);
                Caster.ResetATB((int)Mathf.Ceil(BackSwing));
            }
            
            yield return null;
        }

        public void CancelSequence()
        {
            SequenceState = ActionState.Cancelled;
        }
    }

    [Serializable]
    public class EntityAction
    {
        public ActionState ActionState { get; private set; }

        public ActionType ActionType { get; set; } = ActionType.Ability;

        public string ActionId { get; set; }

        public Entity Caster { get; }
        public List<Entity> Targets { get; set; } = new List<Entity>();

        public float CastTime { get; private set; }
        private float m_delay = 1.0f;
        private float m_endDelay = 0.75f;

        public event ActionStartedHandler ActionStarted;
        public delegate void ActionStartedHandler(string display);

        private InventoryManager m_InventoryManager;

        public event ActionDamageAppliedHandler ActionDamageApplied;
        public delegate void ActionDamageAppliedHandler(Entity target, Enums.Attribute attribute, int value);

        public EntityAction(Entity caster, ActionType actionType, string actionId, List<Entity> targets, InventoryManager manager = null)
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
        private List<Entity> GetTargets(TargetType targetType, List<PlayableCharacter> allies = null, List<Enemy> enemies = null)
        {
            var targets = new List<Entity>();
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
                    ActionStarted($"{Caster.Name} uses {GetActionName()} on {string.Join(", ", Targets.Select(x => x.Name))}");
                    var ability = AbilitiesCollector.TryGetAbility(ActionId);
                    yield return new WaitForSeconds(m_delay);
                    if(ActionState != ActionState.Cancelled)
                    {
                        foreach (var resourceCost in ability.CastCost)
                        {
                            Caster.ApplyDamage(resourceCost.Key, resourceCost.Value);
                        }
                        effects = ability.Effects;
                    }
                    break;
                case ActionType.Item:
                    ActionStarted($"{Caster.Name} uses {GetActionName()} on {string.Join(", ", Targets.Select(x => x.Name))}");
                    effects = ItemCollector.TryGetConsumable(ActionId).Effects;
                    if (m_InventoryManager != null)
                    {
                        m_InventoryManager.RemoveItem(ActionId, 1);
                    }
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
                        foreach (var attribute in effect.Attributes)
                        {
                            List<Entity> targets = new List<Entity>();
                            var attackPower = attribute.Value.Potency;
                            foreach (var scaling in effect.Scalings)
                            {
                                attackPower += Caster.GetAttribute(scaling.Key) * scaling.Value;
                            }

                            foreach (var target in GetTargets(effect.TargetType, allies, enemies))
                            {
                                ActionDamageApplied(target, attribute.Key, (int)Mathf.Ceil((attackPower * -1) * effect.PowerRange.GetValue()));
                            }
                        }
                        break;
                    case EffectType.Cure:
                        foreach(var attribute in effect.Attributes)
                        {
                            List<Entity> targets = new List<Entity>();
                            var curePower = attribute.Value.Potency;
                            foreach (var scaling in effect.Scalings)
                            {
                                curePower += Caster.GetAttribute(scaling.Key) * scaling.Value;
                            }

                            foreach (var target in GetTargets(effect.TargetType, allies, enemies))
                            {
                                if(ActionType == ActionType.Ability || ActionType == ActionType.Item)
                                {
                                    ActionDamageApplied(target, attribute.Key, (int)Mathf.Ceil((curePower) * effect.PowerRange.GetValue()));
                                }
                                else
                                {
                                    target.ApplyDamage(attribute.Key, (int)Mathf.Ceil((curePower) * effect.PowerRange.GetValue()));
                                }
                            }
                        }
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
