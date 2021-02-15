using RPGTest.Collectors;
using RPGTest.Controllers;
using RPGTest.Managers;
using RPGTest.Models;
using RPGTest.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace RPGTest.Interactibles
{
    public class InteractibleMerchant : MonoBehaviour, IInteractible
    {
        public string Id;

        private UIManager m_UIManager => FindObjectOfType<UIManager>();

        private Npc m_npc => NpcCollector.TryGetMerchant(Id);

        private TextMeshPro m_header;

        public void Update()
        {
            if (m_npc != null && m_header == null)
            {
                m_header = GetComponentInChildren<TextMeshPro>();
                m_header.text = m_npc.Greetings;
            }
            if(m_header != null && FindObjectOfType<PlayerController>() != null)
            {
                m_header.gameObject.transform.LookAt((FindObjectOfType<PlayerController>().gameObject.transform.position));
                m_header.gameObject.transform.Rotate(new Vector3(0, 180, 0));
            }
        }

        public void Interact()
        {
            var items = GetMerchantInventory().Select(x => ItemCollector.TryGetItem(x));

            m_UIManager.GetUIComponent<UI_Shop>().Initialize(items);
        }

        private List<string> GetMerchantInventory()
        {
            var items = NpcCollector.TryGetMerchant(Id).Inventory;
            return items;
        }
    }
}
