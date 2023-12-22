using System;
using System.Collections.Generic;
using System.Linq;
using RPGTest.Inputs;
using RPGTest.Managers;
using RPGTest.Models.Entity;
using RPGTest.Modules.Party;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    public class UI_CharacterFilters : UI_View
    {
        [SerializeField] private UI_CharacterFilter CharacterGo;
        [SerializeField] private GameObject CharacterContainer;

        [SerializeField] private Image LeftCycle;
        [SerializeField] private Image RightCycle;

        // Characters control
        private List<UI_CharacterFilter> m_charactersGui = new List<UI_CharacterFilter>();
        private int m_currentCharacterIndex = -1;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        [HideInInspector]
        public event EventHandler<CharacterFilterSelectedEventArgs> CharacterFilterSelected;

        // Input Control
        private string m_actionName = "Cycle";
        private InputDisplayManager m_inputManager => FindObjectOfType<InputDisplayManager>();

        public override void Awake()
        {
            base.Awake();
            m_playerInput.UI.Cycle.started += ctx =>
            {
                Cycle_Performed(ctx);
            };
        }

        public override Dictionary<string, string[]> GetInputDisplay(Controls playerInput = null)
        {
            var controls = playerInput ?? m_playerInput;
            //m_inputActions = new Dictionary<string, string[]>()
            //{
            //    {
            //        "Change Character",
            //        new string[]
            //        {
            //            "UI_" + controls.UI.Cycle.name
            //        }
            //    }
            //};

            return m_inputActions;
        }

        /// <summary>
        /// Create a character filter for every existing party members.
        /// Should be ran once per menu opening
        /// </summary>
        public void Initialize()
        {
            m_currentCharacterIndex = -1;
            foreach (var character in m_partyManager.GetAllExistingPartyMembers())
            {
                var go = Instantiate(CharacterGo);

                go.name = character.Id;
                go.transform.SetParent(CharacterContainer.gameObject.transform);
                go.transform.localScale = new Vector3(1, 1, 1);

                go.GetComponent<UI_CharacterFilter>().Initialize(character);
                go.GetComponent<UI_CharacterFilter>().CharacterFilterSelected += OnCharacter_Selected;
                m_charactersGui.Add(go);
            }
        }

        /// <summary>
        /// To call when the control gets opened
        /// </summary>
        /// <param name="characterIndex">Index of the character in the party list to default on</param>
        public void Open(int characterIndex)
        {
            OnScheme_Changed(null, null);
            m_inputManager.SchemeChanged += OnScheme_Changed;

            m_currentCharacterIndex = characterIndex;
            if (m_partyManager.TryGetPartyMemberAtIndex(characterIndex, out var member))
            {
                ChangeCharacterInternal(member);
            }
            else
            {
                ChangeCharacterInternal(m_partyManager.GetFirstExistingPartyMember());
            }
        }

        /// <summary>
        /// Delete all items and empty the list containing them
        /// </summary>
        public void Clear()
        {
            m_charactersGui.ForEach(x => Destroy(x.gameObject));
            m_charactersGui.Clear();
        }

        /// <summary>
        /// To call when the control gets closed
        /// </summary>
        public override void Close()
        {
            base.Close();
            m_inputManager.SchemeChanged -= OnScheme_Changed;
        }

        /// <summary>
        /// Retrieve character that is currently filtered
        /// </summary>
        /// <returns></returns>
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

        #region input events
        private void Cycle_Performed(InputAction.CallbackContext ctx)
        {
            var movement = ctx.ReadValue<float>();
            ChangeCharacter(movement > 0);
        }
        #endregion

        #region Event
        private void OnCharacter_Selected(object sender, CharacterFilterSelectedEventArgs e)
        {
            ChangeCharacterInternal(e.Character);
        }

        private void OnScheme_Changed(object sender, EventArgs e)
        {
            var actionDisplays = m_inputManager.GetInputDisplays(new Dictionary<string, string[]>()
            {
                {
                    m_actionName,
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cycle.name
                    }
                },
            });

            var icons = actionDisplays.Single((a) => a.Description == m_actionName).Icons;

            LeftCycle.sprite = Sprite.Create(icons[0][0], new Rect(0.0f, 0.0f, icons[0][0].width, icons[0][0].height), new Vector2(0.5f, 0.5f), 100.0f);
            RightCycle.sprite = Sprite.Create(icons[0][1], new Rect(0.0f, 0.0f, icons[0][1].width, icons[0][1].height), new Vector2(0.5f, 0.5f), 100.0f);
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
