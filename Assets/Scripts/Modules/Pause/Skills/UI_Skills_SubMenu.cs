using System.Collections.Generic;
using RPGTest.Managers;
using RPGTest.Models.Entity;
using RPGTest.Modules.Party;
using RPGTest.Modules.UI.Pause.Skills.Controls;
using RPGTest.Modules.UI.Pause.Skills.Views;
using RPGTest.UI;
using RPGTest.UI.Common;
using UnityEngine;

namespace RPGTest.Modules.UI.Pause.Skills
{
    public class UI_Skills_SubMenu : UI_Pause_SubMenu
    {
        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        [SerializeField] private UI_CharacterFilters CharacterFilters;

        [SerializeField] private UI_Skills_SkillTree SkillTree;
        [SerializeField] private UI_Skills_TreeUpdator SkillTreeUpdator;

        public override void Awake()
        {
            base.Awake();
            m_playerInput.UI.Cancel.performed += OnCancel_performed;
            // m_playerInput.UI.InfoToggle.performed += OnInfoToggle_performed;

            CharacterFilters.CharacterFilterSelected += OnCharacter_Selected;
        }

        public override void Initialize()
        {
            // Populate the list of available characters
            CharacterFilters.Initialize();
        }

        public override void OpenSubMenu(Dictionary<string, object> parameters)
        {
            base.OpenSubMenu(parameters);

            var characterIndex = -1;
            if (parameters.TryGetValue("CharacterId", out var value))
            {
                var character = m_partyManager.TryGetPartyMemberById((string)value);
                characterIndex = character != null ? m_partyManager.GetIndexOfPartyMember(character) : -1;
            }

            foreach (var node in FindObjectsOfType<UI_Skills_SkillNode>())
            {
                node.SkillNodeSelected += OnSkillNode_Selected;
            }

            CharacterFilters.Open(characterIndex);
            UpdateInputActions();
        }

        public override void CloseSubMenu()
        {
            foreach (var node in FindObjectsOfType<UI_Skills_SkillNode>())
            {
                node.SkillNodeSelected -= OnSkillNode_Selected;
            }

            base.CloseSubMenu();
        }

        #region Event Handlers
        private void OnCharacter_Selected(object sender, CharacterFilterSelectedEventArgs e)
        {
            //TooltipsView.gameObject.SetActive(false);
            ChangeCharacter(e.Character);
        }

        private void OnSkillNode_Selected(object sender, SkillNodeSelectedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Private Methods
        private void ChangeCharacter(PlayableCharacter character)
        {
            SkillTreeUpdator.Initialize(character);
            SkillTreeUpdator.ExplicitNavigation();

            SkillTree.Select();
        }
        #endregion
    }
}
