using RPGTest.Managers;
using RPGTest.Models;
using RPGTest.Collectors;
using RPGTest.UI;
using System.Linq;
using TMPro;
using UnityEngine;
/*
namespace RPGTest.AI
{
    public class NPCBehavior : MonoBehaviour
    {
        public string ID;
        public bool CanInteract;

        public TextMeshPro TextProMesh;

        private bool m_initialized = false;
        private GameObject m_interactibleObject;

        private NPC m_npc_info;

        private void InitializeNpc()
        {
            TextProMesh.text = m_npc_info.Name;

            m_initialized = true;
        }

        public void Start()
        {
            m_npc_info = NpcCollector.TryGetNpc(ID);

            if(m_npc_info != null)
            {
                InitializeNpc();
            }
        }

        public void Update()
        {
            if(m_npc_info == null)
            {
                m_npc_info = NpcCollector.TryGetNpc(ID);
            }
            if(!m_initialized && m_npc_info != null)
            {
                InitializeNpc();
            }
        }

        #region Trigger Events
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                CanInteract = true;
                m_interactibleObject = other.gameObject;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                CanInteract = false;
                m_interactibleObject = null;
            }
        }
        #endregion
    }
}
*/