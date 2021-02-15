using RPGTest.Collectors;
using RPGTest.Models.Entity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.Managers
{
    public class PartyManager : MonoBehaviour
    {
        public List<GameObject> BattleModels;
        private int m_activePartyThresold = 3;
        List<PlayableCharacter> m_partyMembers = new List<PlayableCharacter>();

        /// <summary>
        /// Gets the active party members
        /// </summary>
        /// <returns>First 3 memebrs of the member list</returns>
        public List<PlayableCharacter> GetActivePartyMembers()
        {
            if(m_partyMembers.Count > 0)
            {
                return m_partyMembers.Where(x => m_partyMembers.IndexOf(x) < m_activePartyThresold).ToList();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the active party members
        /// </summary>
        /// <returns>First 3 memebrs of the member list</returns>
        public List<PlayableCharacter> GetAllPartyMembers()
        {
            if (m_partyMembers.Count > 0)
            {
                return m_partyMembers;
            }
            else
            {
                return null;
            }
        }

        public PlayableCharacter TryGetPartyMember(string id)
        {
            if (m_partyMembers != null && m_partyMembers.Count > 0)
            {
                return m_partyMembers.SingleOrDefault(x => x.Id == id);
            }
            else
            {
                return null;
            }
        }

        public bool TryAddPartyMember(string id)
        {
            var member = PlayableCharactersCollector.TryGetPlayableCharacter(id);
            if(member == null || m_partyMembers.Any(x => x.Id == member.Id))
            {
                return false;
            }
            else
            {
                m_partyMembers.Add(member);
                return true;
            }
        }

        public void SwapMemberPosition(int id1, int id2)
        {
            var member1 = m_partyMembers[id1];
            var member2 = m_partyMembers[id2];

            m_partyMembers[id1] = member2;
            m_partyMembers[id2] = member1;
        }

        public bool TryChangeRow(int id)
        {
            if(id < 3)
            {
                m_partyMembers[id].FrontRow = !m_partyMembers[id].FrontRow;
            }
            return false;
        }
    }
}
