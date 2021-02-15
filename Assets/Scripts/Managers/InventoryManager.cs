using RPGTest.Collectors;
using RPGTest.Models;
using RPGTest.Models.Items;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.Managers
{
    public class InventoryManager : MonoBehaviour
    {
        public int Money { get; private set; } = 100;

        private Dictionary<string, int> m_items = new Dictionary<string, int>();
        private const int m_maxStack = 99;
        private const int m_maxMoneyAmount = 999999;

        public Dictionary<string, int> GetAllItems()
        {
            return m_items;
        }

        /// <summary>
        /// Attempt to add multiple items in the inventory
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public Dictionary<string, int> TryAddItems(Dictionary<string, int> items)
        {
            Dictionary<string, int> leftovers = new Dictionary<string, int>();
            foreach (var item in items)
            {
                var remaining = TryAddItem(item.Key, item.Value);
                if (remaining > 0)
                {
                    leftovers.Add(item.Key, remaining);
                }
            }
            return leftovers;
        }

        /// <summary>
        /// Attempt to add an item in the inventory.
        /// If the item is already present we simply increase the owned quantity.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public int TryAddItem(string ID, int quantity)
        {
            var remainingQuantity = 0;
            if (m_items.ContainsKey(ID))
            {
                var remainingSlots = m_maxStack - m_items[ID];

                if (quantity < remainingSlots)
                {
                    m_items[ID] += quantity;
                }
                else
                {
                    m_items[ID] += remainingSlots;
                    remainingQuantity = quantity - remainingSlots;
                }
            }
            else
            {
                m_items.Add(ID, quantity);
            }
            return remainingQuantity;
        }

        public bool TryGetItem(string id, out Item item)
        {
            if(m_items.ContainsKey(id))
            {
                item = ItemCollector.TryGetItem(id);
                return true;
            }
            item = null;
            return false;
        }

        public List<Consumable> GetConsumables()
        {
            List<Consumable> consumables = new List<Consumable>();
            foreach(var item in m_items)
            {
                if(TryGetItem(item.Key, out Item cons))
                {
                    if (cons.Type == Enums.ItemType.Consumable)
                        consumables.Add((Consumable)cons);
                }
            }
            return consumables;
        }

        /// <summary>
        /// Returns the amount number of items held in an item stack
        /// </summary>
        /// <param name="ID">Item ID to find the slot for said object</param>
        /// <returns>Number of held items in an item stack</returns>
        public int GetHeldItemQuantity(string ID)
        {
            if (m_items.ContainsKey(ID))
            {
                return m_items[ID];
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the remaining slots available for an item stack
        /// </summary>
        /// <param name="ID">Item ID to find the slots for said object</param>
        /// <returns>Number of slots remaining in an item stack</returns>
        public int GetRemainingItemSlots(string ID)
        {
            if(m_items.ContainsKey(ID))
            {
                return m_maxStack - m_items[ID];
            }
            else
            {
                return m_maxStack;
            }
        }

        public void RemoveItem(string ID, int quantity)
        {
            if (m_items.ContainsKey(ID))
            {
                m_items[ID] -= quantity;
                if(m_items[ID] <= 0)
                {
                    m_items.Remove(ID);
                }
            }
        }

        /// <summary>
        /// Tries to update the amount of money the player have in their inventory.
        /// Fails if the change brings the amount below 0
        /// Succeeds if the change keeps the amount above 0
        /// </summary>
        /// <param name="amount">Variation of money</param>
        /// <returns>True if the amount was modified, false otherwise</returns>
        public bool TryUpdateMoney(int amount)
        {
            if(Money + amount < 0)
            {
                return false;
            }
            else if(Money + amount > m_maxMoneyAmount)
            {
                Money = m_maxMoneyAmount;
                return true;
            }
            else
            {
                Money += amount;
                return true;
            }
        }
    }
}
