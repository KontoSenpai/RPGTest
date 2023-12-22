using System.Collections.Generic;
using RPGTest.Managers;
using RPGTest.Modules.Party;
using UnityEngine;

namespace RPGTest.UI
{
    public class UI_Skills_SubMenu : UI_Pause_SubMenu
    {
        private List<GameObject> m_allMembers = new List<GameObject>();
        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        public override void Initialize()
        {

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
        }
    }
}
