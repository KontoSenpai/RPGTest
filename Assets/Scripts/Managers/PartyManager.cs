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
        /// Gets all the party members, regardless of their existence
        /// </summary>
        /// <returns>All party members. Active and Inactive, null and not null</returns>
        public List<PlayableCharacter> GetAllPartyMembers()
        {
            return m_partyMembers.ToList();
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

        /// <summary>
        /// Gets the first existing party member
        /// </summary>
        /// <returns>The first party member that is not null</returns>
        public PlayableCharacter GetFirstExistingPartyMember()
        {
            return GetAllPartyMembers().First((c) => c != null);
        }

        /// <summary>
        /// Gets all party members that are not null
        /// </summary>
        /// <returns>List of non null PlayableCharacters</returns>
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

        public PlayableCharacter GetGuestCharacter()
        {
            return null;
        }

        public bool TryGetPartyMemberAtIndex(int index, out PlayableCharacter character)
        {
            character = null;
            if (index < 0 || index >= m_partyMembers.Length) return false; 

            character = m_partyMembers[index];
            return character != null;
        }

        public PlayableCharacter TryGetPartyMemberById(string id)
        {
            return m_partyMembers.SingleOrDefault(m => m != null && m.Id == id);
        }

        public List<PlayableCharacter> GetAllExistingPartyMembers()
        {
            return GetAllPartyMembers().Where(m => m != null).ToList();
        }

        public int GetIndexOfFirstExistingPartyMember()
        {
            return GetAllPartyMembers().FindIndex(0, c => c != null);
        }

        public int GetIndexofLastExistingPartyMember()
        {
            return GetAllPartyMembers().FindLastIndex(c => c != null);
        }

        public int GetIndexOfPartyMember(PlayableCharacter c)
        {
            return GetAllPartyMembers().IndexOf(c);
        }

        public int GetIndexOfFirstExistingCharacterFromIndex(int startingIndex, bool isIncreasing)
        {
            if (isIncreasing)
            {
                if (startingIndex < m_partyMembersArraySize)
                {
                    for (int i = startingIndex + 1; i < m_partyMembersArraySize; i++)
                    {
                        if (m_partyMembers[i] != null)
                        {
                            return i;
                        }
                    }
                }
                return GetIndexOfFirstExistingCharacterFromIndex(-1, isIncreasing);
            }
            else
            {
                if (startingIndex > 0)
                {
                    for (int i = startingIndex - 1; i >= 0; i--)
                    {
                        if (m_partyMembers[i] != null)
                        {
                            return i;
                        }
                    }
                }
                return GetIndexOfFirstExistingCharacterFromIndex(m_partyMembersArraySize, isIncreasing);
            }
        }

        public bool TryAddPartyMember(string id)
        {
            var member = PlayableCharactersCollector.TryGetPlayableCharacter(id);
            if(member != null && TryGetPartyMemberById(member.Id) == null)
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

        public void SwapCharactersPosition(int index1, int index2)
        {
            PerformSwap(index1, index2);
            FindAndFixHoles(m_activePartyThreshold);
        }

        public void PerformSwap(int id1, int id2)
        {
            var member1 = m_partyMembers[id1];
            var member2 = m_partyMembers[id2];

            m_partyMembers[id1] = member2;
            m_partyMembers[id2] = member1;
        }

        // Fix any potential holes between 2 members after a swap
        // TODO : prevent inactive to fill holes in primary, and vice-versa
        private void FindAndFixHoles(int startIndex)
        {
            var firstEmptyIndex = -1;
            for (int i = startIndex; i < m_partyMembers.ToList().Count; i++)
            {
                TryGetPartyMemberAtIndex(i, out var member);
                if (member == null && firstEmptyIndex == -1)
                {
                    firstEmptyIndex = i;
                }
                else if (member != null && firstEmptyIndex != -1)
                {
                    PerformSwap(firstEmptyIndex, i);
                    i = firstEmptyIndex++;
                    firstEmptyIndex = -1;
                }
            }
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
