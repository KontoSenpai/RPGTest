using RPGTest.Managers;
using RPGTest.Collectors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.Interactibles
{
    public class InteractibleChest : MonoBehaviour, IInteractible
    {
        public string Id;
        public Material Chest;
        public Material ChestOpened;

        private Dictionary<string, int> m_items;

        public void Interact()
        {
            var inventoryManager = FindObjectOfType<InventoryManager>();
            var leftovers = inventoryManager.TryAddItems(GetChestInventory());

            if (leftovers.Any())
            {
                m_items = leftovers;
            }
            else
            {
                this.GetComponentInChildren<MeshRenderer>().material = ChestOpened;
                gameObject.GetComponents<Collider>().SingleOrDefault(x => x.isTrigger).enabled = false;
            }
        }

        public void UpdateInventory(Dictionary<string, int> inventory)
        {
            m_items = inventory;
            if (m_items == null || m_items.Count == 0)
            {
                this.GetComponentInChildren<MeshRenderer>().material = ChestOpened;
                gameObject.GetComponents<Collider>().SingleOrDefault(x => x.isTrigger).enabled = false;
            }
        }

        public Dictionary<string, int> GetChestInventory()
        {
            if(m_items == null)
            {
                m_items = InteractiblesCollector.TryGetChest(Id).Inventory;
            }

            return m_items;
        }
    }
}
