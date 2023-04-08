using RPGTest.Collectors;
using RPGTest.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.Managers
{
    public class PartyManager : MonoBehaviour
    {
        public List<GameObject> BattleModels;
        private static readonly int m_activePartyThreshold = 3;
        private static readonly int m_partyMembersArraySize = 11;
        private PlayableCharacter[] m_partyMembers = new PlayableCharacter[m_partyMembersArraySize];

        public int GetActivePartyThreshold()
        {
            return m_activePartyThreshold;
        }

        /// <summary>
        /// Gets the active party members
        /// </summary>
        /// <returns>First 3 memebrs of the member list</returns>
        public List<PlayableCharacter> GetActivePartyMembers()
        {
            List<PlayableCharacter> characters = new List<PlayableCharacter>();
            for(int index = 0; index < m_activePartyThreshold; index++)
            {
                characters.Add(m_partyMembers[index]);
            }
            return characters;
        }

        public List<PlayableCharacter> GetExistingActivePartyMembers()
        {
            return GetActivePartyMembers().Where(m => m != null).ToList();
        }

        public List<PlayableCharacter> GetInactivePartyMembers()
        {
            return m_partyMembers.Skip(m_activePartyThreshold).ToList();
        }
        public List<PlayableCharacter> GetExistingInactivePartyMembers()
        {
            return GetInactivePartyMembers().Where(m => m != null).ToList();
        }

        /// <summary>
        /// Gets the active party members
        /// </summary>
        /// <returns>All party members. Active and Inactive</returns>
        public List<PlayableCharacter> GetAllPartyMembers()
        {
            return m_partyMembers.ToList();
        }

        public PlayableCharacter GetGuestCharacter()
        {
            return null;
        }

        public PlayableCharacter GetPartyMemberAtIndex(int index)
        {
            if (index < m_partyMembers.Length)
            {
                return m_partyMembers[index];
            }
            return null;
        }

        public List<PlayableCharacter> GetAllExistingPartyMembers()
        {
            return GetAllPartyMembers().Where(m => m != null).ToList();
        }

        public int GetIndexofLastExistingPartyMember()
        {
            return GetAllPartyMembers().FindLastIndex(c => c != null);
        }

        public PlayableCharacter TryGetPartyMember(string id)
        {
            return m_partyMembers.SingleOrDefault(m => m != null && m.Id == id);
        }

        public bool TryAddPartyMember(string id)
        {
            var member = PlayableCharactersCollector.TryGetPlayableCharacter(id);
            if(member != null && TryGetPartyMember(member.Id) == null)
            {
                for(int index = 0; index < m_partyMembers.Count(); index++)
                {
                    if(m_partyMembers[index] == null)
                    {
                        m_partyMembers[index] = member;
                        return true;
                    }
                }
            }
            return false;
        }

        public void PerformSwap(int id1, int id2)
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
