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
        [SerializeField] private GameObject[] ActiveMembers;
        [SerializeField] private GameObject[] InactiveMembers;
        [SerializeField] private TextMeshProUGUI ExperienceGainedValue;

        [SerializeField] private GameObject ItemsList;
        [SerializeField] private GameObject ItemItemInstantiate;

        [SerializeField] private TextMeshProUGUI GoldGainedValue;

        private string m_ExperienceGainedString = "Gained {0} EXP";
        private string m_GoldGainedString = "Gained {0} G";
        private bool m_ActiveGain = true;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;
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

            List<PlayableCharacter> activeMembers = m_partyManager.GetActivePartyMembers();
            for(int i = 0; i < activeMembers.Count; i++)
            {
                ActiveMembers[i].SetActive(activeMembers[i] != null);
                if(ActiveMembers[i].activeInHierarchy)
                {
                    UI_Member_Widget widgetScript = ActiveMembers[i].GetComponent<UI_Member_Widget>();
                    widgetScript.Initialize(activeMembers[i]);
                    activeMembers[i].PlayerExperienceChanged += widgetScript.RefreshExperience;
                }
            }

            List<PlayableCharacter> inactiveMembers = m_partyManager.GetInactivePartyMembers();
            for (int i = 0; i < inactiveMembers.Count - activeMembers.Count; i++)
            {
                InactiveMembers[i].SetActive(inactiveMembers[i] != null);
                if (InactiveMembers[i].activeInHierarchy)
                {
                    UI_Member_Widget widgetScript = InactiveMembers[i].GetComponent<UI_Member_Widget>();
                    widgetScript.Initialize(inactiveMembers[i]);
                    inactiveMembers[i].PlayerExperienceChanged += widgetScript.RefreshExperience;
                }
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
                    foreach(PlayableCharacter member in m_partyManager.GetExistingInactivePartyMembers().Where(p => p.IsAlive))
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
            List<PlayableCharacter> activeMembers = m_partyManager.GetActivePartyMembers();
            for (int i = 0; i < activeMembers.Count; i++)
            {
                if (ActiveMembers[i].activeInHierarchy)
                {
                    activeMembers[i].PlayerExperienceChanged -= ActiveMembers[i].GetComponent<UI_Member_Widget>().RefreshExperience;
                }
            }

            List<PlayableCharacter> inactiveMembers = m_partyManager.GetInactivePartyMembers();
            for (int i = 0; i < inactiveMembers.Count - activeMembers.Count; i++)
            {
                if (InactiveMembers[i].activeInHierarchy)
                {
                    inactiveMembers[i].PlayerExperienceChanged -= InactiveMembers[i].GetComponent<UI_Member_Widget>().RefreshExperience;
                }
            }

            m_allItems.ForEach(i => Destroy(i));
            m_allItems.Clear();
        }
    }
}