using RPGTest.Models.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.Modules.Battle.Action
{
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
            if (currentAction != null)
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
            foreach (EntityAction action in Actions)
            {
                // Return early if one action has exited early
                if (Actions.Any(a => a.ActionState == ActionState.Cancelled))
                {
                    SequenceState = ActionState.Completed;
                    action.CancelCast();
                    break;
                }
                manager.StartCoroutine(action.Execute(allies, enemies));
                while (action.ActionState == ActionState.Executing)
                {
                    if (SequenceState == ActionState.Cancelled)
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
                    yield break;
                }
                else if (allies.TrueForAll(x => !x.IsAlive))
                {
                    Debug.Log("LOSE");
                    ActionSequenceCompleted(this);
                    yield break;
                }
            }

            if (Actions.All(a => a.ActionState == ActionState.Completed))
            {
                SequenceState = ActionState.Completed;
            }

            if (SequenceState == ActionState.Completed)
            {
                ActionSequenceCompleted(this);
                Caster.ResetATB((int)Mathf.Ceil(BackSwing));
            }

            yield return null;
        }

        public void CancelSequence()
        {
            SequenceState = ActionState.Cancelled;
        }
    }
}
