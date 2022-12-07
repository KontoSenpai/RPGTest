using RPGTest.Enums;
using System.Collections.Generic;
using System.Linq;

namespace RPGTest.Modules.Battle
{
    public partial class TargettingSystem
    {
        private bool CanSelectSingleTarget()
        {
            return m_possibleTargets.Any(t => t == TargetType.SingleAlly || t == TargetType.SingleEnemy);
        }

        private void SelectSingleTarget(bool positive)
        {
            var maxIndex = m_targetEnemy ? m_enemies.Where(e => e.IsAlive).Count() - 1 : m_party.Count - 1;

            if (m_targetAll)
            {
                m_targetIndex = positive ? 0 : maxIndex;
                return;
            }

            if (positive && m_targetIndex < maxIndex)
            {
                m_targetIndex++;
            } else if (!positive && m_targetIndex > 0)
            {
                m_targetIndex--;
            }
        }
    }
}
