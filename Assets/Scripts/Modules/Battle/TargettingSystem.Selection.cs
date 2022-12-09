using RPGTest.Enums;
using RPGTest.Models.Entity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.Modules.Battle
{
    public partial class TargettingSystem
    {
        private bool CanSelectSingleTarget()
        {
            return m_possibleTargets.Any(t => t == TargetType.SingleAlly || t == TargetType.SingleEnemy);
        }

        private bool CanSelectAllEnemies()
        {
            return m_possibleTargets.Any(t => t == TargetType.Enemies);
        }

        private bool CanSelectSingleEnemy()
        {
            return m_possibleTargets.Any(t => t == TargetType.SingleEnemy);
        }

        private bool CanSelectSingleAlly()
        {
            return m_possibleTargets.Any(t => t == TargetType.SingleAlly);
        }

        private bool CanSelectAllAllies()
        {
            return m_possibleTargets.Any(t => t == TargetType.Allies);
        }

        private int GetNextValidIndex(List<Enemy> entities, bool positive)
        {
            if (positive)
            {
                for (int i = m_targetIndex; i < entities.Count; i++)
                {
                    if (entities[i].IsAlive)
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (int i = m_targetIndex; i > 0; i--)
                {
                    if (entities[i].IsAlive)
                    {
                        return i;
                    }
                }
            }

            return m_targetIndex;
        }

        private void SelectSingleTarget(bool positive)
        {
            var minIndex = m_targetEnemy ? m_enemies.GetFirstAliveIndex() : 0;
            var maxIndex = m_targetEnemy ? m_enemies.GetLastAliveIndex() : m_party.Count - 1;
            var newIndex = m_targetEnemy ? GetNextValidIndex(m_enemies, positive) : 
                positive ? m_targetIndex + 1 : 
                m_targetIndex - 1;

            if (m_targetAll)
            {
                m_targetIndex = positive ? minIndex : maxIndex;
                return;
            }
            if (newIndex >= minIndex && newIndex <= maxIndex)
            {
                m_targetIndex = newIndex;
            }
        }

        private void SelectOtherSide()
        {
            // From all enemies to single enemy
            if (m_targetEnemy && m_targetSide && CanSelectSingleEnemy())
            {
                m_targetSide = false;
                m_targetIndex = m_enemies.GetFirstAliveIndex();

            }
            // From all enemies to single ally
            else if (m_targetEnemy && CanSelectSingleAlly())
            {
                m_targetEnemy = false;
                m_targetSide = false;
                m_targetIndex = 0;
            }
            // From all enemies to all allies
            else if (m_targetEnemy && CanSelectAllAllies())
            {
                m_targetEnemy = false;
                m_targetSide = true;
                m_targetIndex = -1;
            }
            // From all allies to single ally
            else if(!m_targetEnemy && m_targetSide && CanSelectSingleAlly())
            {
                m_targetSide = false;
                m_targetIndex = 0;
            }
            // From all allies to single enemy
            else if (!m_targetEnemy && CanSelectSingleEnemy())
            {
                m_targetEnemy = true;
                m_targetSide = false;
                m_targetIndex = m_enemies.GetFirstAliveIndex();
            }
            // From all allies to all enemies
            else if (!m_targetEnemy && CanSelectAllEnemies())
            {
                m_targetEnemy = true;
                m_targetSide = true;
                m_targetIndex = -1;
            }
        }
    }
}
