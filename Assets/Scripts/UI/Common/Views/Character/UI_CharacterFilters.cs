using RPGTest.Managers;
using RPGTest.Models.Entity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    public class UI_CharacterFilters : MonoBehaviour
    {
        [SerializeField] private UI_CharacterFilter CharacterGo;
        [SerializeField] private GameObject CharacterContainer;

        // Characters control
        private List<UI_CharacterFilter> m_charactersGui = new List<UI_CharacterFilter>();
        private int m_currentCharacterIndex = -1;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        [HideInInspector]
        public event EventHandler<CharacterFilterSelectedEventArgs> CharacterFilterSelected;

        public void Initialize(int characterIndex)
        {
            m_currentCharacterIndex = characterIndex;
            foreach (var character in m_partyManager.GetAllPartyMembers())
            {
                if (character == null)
                    continue;

                var go = Instantiate(CharacterGo);

                go.name = character.Id;
                go.transform.SetParent(CharacterContainer.gameObject.transform);
                go.transform.localScale = new Vector3(1, 1, 1);

                go.GetComponent<UI_CharacterFilter>().Initialize(character);
                go.GetComponent<UI_CharacterFilter>().CharacterFilterSelected += OnCharacter_Selected;
                m_charactersGui.Add(go);
            }

            if (m_partyManager.TryGetPartyMemberAtIndex(characterIndex, out var member))
            {
                ChangeCharacterInternal(member);
            } else
            {
                ChangeCharacterInternal(m_partyManager.GetFirstExistingPartyMember());
            }
        }

        public void Clear()
        {
            m_charactersGui.ForEach((g) => Destroy(g.gameObject));
            m_charactersGui.Clear();
        }

        public PlayableCharacter GetCurrentCharacter()
        {
            if (m_partyManager.TryGetPartyMemberAtIndex(m_currentCharacterIndex, out var character)) {
                return character;
            }

            return null;
        }

        public void ChangeCharacter(bool increasing)
        {
            var newIndex = m_partyManager.GetIndexOfFirstExistingCharacterFromIndex(m_currentCharacterIndex, increasing);
            if (newIndex == m_currentCharacterIndex) return;

            if (m_partyManager.TryGetPartyMemberAtIndex(newIndex, out var character))
            {
                ChangeCharacterInternal(character);
            }
        }

        #region Event
        private void OnCharacter_Selected(object sender, CharacterFilterSelectedEventArgs e)
        {
            ChangeCharacterInternal(e.Character);
        }
        #endregion

        private void ChangeCharacterInternal(PlayableCharacter character)
        {
            m_charactersGui.ForEach((c) => c.GetComponent<Button>().interactable = c.GetCharacter() != character);
            m_currentCharacterIndex = m_partyManager.GetIndexOfPartyMember(character);

            CharacterFilterSelected.Invoke(this, new CharacterFilterSelectedEventArgs(character));
        }
    }
}
