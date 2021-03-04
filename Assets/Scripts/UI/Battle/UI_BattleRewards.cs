using RPGTest.Inputs;
using RPGTest.Collectors;
using RPGTest.Managers;
using RPGTest.Models.Entity;
using RPGTest.UI.InventoryMenu;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace RPGTest.UI.Battle
{
    public class UI_BattleRewards : UI_Base
    {
        public TextMeshProUGUI ExperienceGainedValue;
        public GameObject PartyList;
        public GameObject PartyItemInstantiate;

        public GameObject ItemsList;
        public GameObject ItemItemInstantiate;

        public TextMeshProUGUI GoldGainedValue;

        private string m_ExperienceGainedString = "Gained {0} EXP";
        private string m_GoldGainedString = "Gained {0} G";
        private bool m_ActiveGain = true;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;
        private List<GameObject> m_allGuiMembers = new List<GameObject>();
        private List<GameObject> m_allItems = new List<GameObject>();

        protected Controls m_playerInput;

        private int m_expTick = 3;

        public void Awake()
        {
            m_playerInput = new Controls();
        }

        public void OnEnable()
        {
            m_playerInput.Enable();
            m_playerInput.UI.Submit.performed += Submit_performed;
        }

        public void OnDisable()
        {
            m_playerInput.Disable();
            m_playerInput.UI.Submit.performed -= Submit_performed;
        }

        public void DisplayRewards(int experience, Dictionary<string, int> items, int gold)
        {
            m_ActiveGain = true;
            
            ExperienceGainedValue.text = string.Format(m_ExperienceGainedString, experience);

            foreach(var member in m_partyManager.GetExistingActivePartyMembers())
            {
                GameObject uiItem = InstantiateItemInViewport(PartyItemInstantiate, member.Id, PartyList);
                UI_Member_Widget widgetScript = uiItem.GetComponent<UI_Member_Widget>();
                widgetScript.Initialize(member);
                member.PlayerExperienceChanged += widgetScript.RefreshExperience;
                m_allGuiMembers.Add(uiItem);
            }

            foreach(var item in items)
            {
                var i = ItemCollector.TryGetItem(item.Key);
                if (i != null)
                {
                    GameObject uiItem = InstantiateItemInViewport(ItemItemInstantiate, item.Key, ItemsList);

                    UI_SubMenu_Inventory_Item widgetScript = uiItem.GetComponent<UI_SubMenu_Inventory_Item>();
                    widgetScript.Initialize(i, item.Value);
                    m_allItems.Add(uiItem);
                }
            }

            GoldGainedValue.text = string.Format(m_GoldGainedString, gold);

            StartCoroutine(DeployRewards(experience));
        }

        public IEnumerator DeployRewards(int experience)
        {
            var allocated = 0;
            do
            {
                if(m_ActiveGain)
                {
                    foreach(PlayableCharacter member in m_partyManager.GetExistingActivePartyMembers().Where(p => p.IsAlive))
                    {
                        member.AddExperience(m_expTick);
                    }
                    allocated += m_expTick;
                    yield return new WaitForSeconds(0.05f);
                }
                else
                {
                    experience -= allocated;
                    foreach (PlayableCharacter member in m_partyManager.GetExistingActivePartyMembers().Where(p => p.IsAlive))
                    {
                        member.AddExperience(experience);
                    }
                    break;
                }
            } while (allocated < experience);

            m_ActiveGain = false;
            yield return null;
        }

        public void Submit_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(m_ActiveGain)
            {
                m_ActiveGain = false;
            }
            else
            {
                Clear();
                UIClosed(this, null);
            }
        }


        public void Clear()
        {
            foreach(PlayableCharacter p in m_partyManager.GetActivePartyMembers())
            {
                var guiItem = m_allGuiMembers.SingleOrDefault(g => g.name == p.Id);
                if(guiItem != null)
                {
                    p.PlayerExperienceChanged -= guiItem.GetComponent<UI_Member_Widget>().RefreshExperience;
                }
            }
            m_allGuiMembers.ForEach(m => Destroy(m));
            m_allGuiMembers.Clear();
            m_allItems.ForEach(i => Destroy(i));
            m_allItems.Clear();
        }
    }
}